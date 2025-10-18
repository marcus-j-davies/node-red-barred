const { Server } = require('socket.io');
module.exports = function (RED) {
	function BarredConfig(config) {
		RED.nodes.createNode(this, config);
		const self = this;
		const connectedScanners = {};
		const barcodeEmitters = {};
		const itemEmitters = {};

		self.registerBarcodeEmitter = (id, fn) => {
			barcodeEmitters[id] = fn;
		};

		self.unregisterBarcodeEmitter = (id) => {
			delete barcodeEmitters[id];
		};

		self.registerItemEmitter = (id, fn) => {
			itemEmitters[id] = fn;
		};

		self.unregisterItemEmitter = (id) => {
			delete itemEmitters[id];
		};

		const ioOptions = {
			path: `/barred-${self.id}/`,
			origin: '*',
			methods: ['GET', 'POST']
		};
		self.io = new Server(RED.server, ioOptions);

		config.scanners = {
			scanner123: 'Test'
		};

		self.io.use((socket, next) => {
			const { id } = socket.handshake.auth;
			if (!config.scanners[id]) {
				return next(new Error('Unauthorized'));
			}
			next();
		});

		self.io.on('connection', (scanner) => {
			connectedScanners[scanner.id] = scanner;

			scanner.on('BARRED.Item', (args, callback) => {
				const msg = {
					_barredCB: callback,
					topic: args.item.type,
					payload: {
						timestamp: args.timestmp,
						item: { ...args.item },
						scanner: {
							id: args.scanner.scannerId,
							appVersion: args.scanner.appVersion
						}
					}
				};

				Object.values(itemEmitters).forEach((emitter) => emitter(msg));
			});

			scanner.on('BARRED.Barcode', (args, callback) => {
				const msg = {
					_barredCB: {
						expires: new Date().getTime() + parseInt(config.rtimeout),
						callback: callback
					},
					topic: args.barcode.barcode,
					payload: {
						timestamp: args.timestamp,
						barcode: { ...args.barcode },
						scanner: {
							id: args.scanner.id,
							appVersion: args.scanner.appVersion
						}
					}
				};

				Object.values(barcodeEmitters).forEach((emitter) => emitter(msg));
			});

			scanner.on('disconnect', () => {
				delete connectedScanners[scanner.id];
			});
		});

		self.on('close', (_, done) => {
			const ns = `barred-${self.id}`;
			const namespace = self.io.of(ns);

			if (namespace.sockets) {
				for (const [id, socket] of namespace.sockets) {
					socket.disconnect(true);
				}
			}

			Object.keys(connectedScanners).forEach((id) => delete connectedScanners[id]);
			namespace.removeAllListeners();

			if (self.io._nsps && self.io._nsps.has(ns)) {
				self.io._nsps.delete(ns);
			}

			done();
		});
	}

	RED.nodes.registerType('barred-config', BarredConfig);
};

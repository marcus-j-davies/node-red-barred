const { Server } = require('socket.io');
module.exports = function (RED) {
	function Config(Config) {
		RED.nodes.createNode(this, Config);
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
			path: `barred-${self.id}`
		};
		self.io = new Server(RED.server, ioOptions);

		self.io.use((socket, next) => {
			const { id } = socket.handshake.auth;
			if (!Config.scanners[id]) {
				return next(new Error('Unauthorized'));
			}
			next();
		});

		self.io.on('connection', (scanner) => {
			connectedScanners[scanner.id] = scanner;

			scanner.on('BARRED.Item', (args) => {
				const msg = {
					_socket: scanner,
					topic: args.type,
					payload: {
						timestamp: args.timestmp,
						item: { ...args.item },
						scanner: {
							id: args.scannerId,
							appVersion: args.appVersion
						}
					}
				};

				Object.values(itemEmitters).forEach((emitter) => emitter(msg));
			});

			scanner.on('BARRED.Barcode', (args) => {
				const msg = {
					_socket: scanner,
					topic: args.barcode.barcode,
					payload: {
						timestamp: args.timestmp,
						barcode: { ...args.barcode },
						scanner: {
							id: args.scannerId,
							appVersion: args.appVersion
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
			self.io.sockets.sockets.forEach((socket) => {
				socket.disconnect(true);
			});
			Object.keys(connectedScanners).forEach((id) => delete connectedScanners[id]);
			self.io.close(() => {
				done();
			});
		});
	}

	RED.nodes.registerType('barred-config', Config);
};

/*
const socket = io('/your-path', {
	path: '/your-node-id',
	auth: {
		id: 'scanner123'
	}
});
*/

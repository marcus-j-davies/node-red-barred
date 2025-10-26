/* eslint-disable no-unused-vars */
const { Server } = require('socket.io');
const path = require('path');
const fs = require('fs');
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
		self.io = new Server(ioOptions);
		self.io.listen(config.port);

		self.io.use((socket, next) => {
			const { id } = socket.handshake.auth;
			if (!config.scanners[id]) {
				return next(new Error('Unauthorized'));
			}
			next();
		});

		self.io.on('connection', (scanner) => {
			connectedScanners[scanner.id] = scanner;

			scanner.on('BARRED.Item', (args) => {
				const msg = {
					payload: {
						timestamp: args.timestamp,
						item: { ...args.item },
						scanner: { ...args.scanner }
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
					payload: {
						timestamp: args.timestamp,
						barcode: { ...args.barcode },
						scanner: { ...args.scanner }
					}
				};

				Object.values(barcodeEmitters).forEach((emitter) => emitter(msg));
			});

			scanner.on('disconnect', () => {
				delete connectedScanners[scanner.id];
			});
		});

		self.on('close', (_, done) => {
			self.io.sockets.sockets.forEach((socket) => socket.disconnect(true));
			self.io.close(done);
		});
	}

	RED.nodes.registerType('barred-config', BarredConfig);

	RED.httpAdmin.get('/barred-api/getversion', (request, response) => {
		response.json({ version: require('../package.json').version });
	});

	RED.httpAdmin.get('/barred-api/geticon/:instanceid', (request, response) => {});
	RED.httpAdmin.post('/barred-api/seticon/:instanceid', (request, response) => {
		const Root = path.join(RED.settings.userDir || '', 'barred', request.params.instanceid);

		if (!fs.existsSync(Root)) {
			fs.mkdirSync(Root, { recursive: true });
		}

		response.status(200).send({ message: 'Directory ready' });
	});
};

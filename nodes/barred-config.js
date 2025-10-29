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
		const actionEmitters = {};

		// Barcodes
		self.registerBarcodeEmitter = (id, fn) => {
			barcodeEmitters[id] = fn;
		};

		self.unregisterBarcodeEmitter = (id) => {
			delete barcodeEmitters[id];
		};

		// Items
		self.registerItemEmitter = (id, fn) => {
			itemEmitters[id] = fn;
		};

		self.unregisterItemEmitter = (id) => {
			delete itemEmitters[id];
		};

		// Actions
		self.registerActionEmitter = (id, fn) => {
			actionEmitters[id] = fn;
		};

		self.unregisterActionEmitter = (id) => {
			delete actionEmitters[id];
		};

		self.sendToScanner = (id, payload) => {
			const PL = {
				payloadType: typeof payload,
				payload: payload
			};

			if (id) {
				if (connectedScanners[id]) {
					connectedScanners[id].emit('BARRED.Item', PL);
				}
			} else {
				Object.values(connectedScanners).forEach((S) => {
					S.emit('BARRED.Item', PL);
				});
			}
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
			connectedScanners[scanner.handshake.auth.id] = scanner;

			scanner.on('BARRED.Item', (args) => {
				const msg = {
					_barredCB: {
						expires: new Date().getTime() + parseInt(config.rtimeout),
						callback: callback
					},
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

			scanner.on('BARRED.Action', (args, callback) => {
				const msg = {
					_barredCB: {
						barcode: args.action.barcode,
						expires: new Date().getTime() + parseInt(config.rtimeout),
						callback: callback
					},
					payload: {
						timestamp: args.timestamp,
						action: { ...args.action },
						scanner: { ...args.scanner }
					}
				};

				Object.values(actionEmitters).forEach((emitter) => emitter(msg));
			});

			scanner.on('disconnect', () => {
				delete connectedScanners[scanner.handshake.auth.id];
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

	RED.httpAdmin.get('/barred-api/geticon/:instanceid', (request, response) => {
		const File = 'icon.png';
		const Root = path.join(RED.settings.userDir || '', 'barred', request.params.instanceid);

		response.setHeader('Content-Type', 'image/png');
		if (fs.existsSync(path.join(Root, File))) {
			response.sendFile(path.join(Root, File));
		} else {
			response.sendFile(path.join(__dirname, '../', 'resources', 'node_red_icon.png'));
		}
	});
	RED.httpAdmin.post('/barred-api/seticon/:instanceid', (request, response) => {
		const File = 'icon.png';
		const Root = path.join(RED.settings.userDir || '', 'barred', request.params.instanceid);

		if (!fs.existsSync(Root)) {
			fs.mkdirSync(Root, { recursive: true });
		}

		const chunks = [];
		request.on('data', (chunk) => {
			chunks.push(chunk);
		});

		request.on('end', () => {
			const buffer = Buffer.concat(chunks);

			if (buffer.subarray(0, 8).toString('hex') !== '89504e470d0a1a0a') {
				response.status(400).send('Invalid PNG file');
				return;
			}

			fs.writeFile(path.join(Root, File), buffer, (err) => {
				if (err) {
					response.status(500).send('Failed to save file');
				} else {
					response.status(200).send('Icon saved successfully');
				}
			});
		});
	});
};

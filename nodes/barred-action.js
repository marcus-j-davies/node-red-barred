module.exports = function (RED) {
	function BarredAction(config) {
		RED.nodes.createNode(this, config);
		const self = this;

		self.config = config;
		self.stack = RED.nodes.getNode(self.config.stack);

		self.stack.registerBarcodeEmitter(self.id, (msg) => {
			self.send(msg);
		});

		self.on('close', (_, done) => {
			self.stack.unregisterBarcodeEmitter(self.id);
			done();
		});
	}

	RED.nodes.registerType('barred-action', BarredAction);
};

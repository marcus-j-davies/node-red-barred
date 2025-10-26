module.exports = function (RED) {
	function BarredPrompt(config) {
		RED.nodes.createNode(this, config);
		const self = this;

		self.config = config;
		self.stack = RED.nodes.getNode(self.config.stack);

		self.on('input', (msg, send, done) => {
			self.stack.sendToScanner(msg.topic, msg.payload);
			send(msg);
			done();
		});
	}

	RED.nodes.registerType('barred-prompt', BarredPrompt);
};

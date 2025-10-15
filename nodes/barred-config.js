module.exports = function (RED) {
	function Config(Config) {
		RED.nodes.createNode(this, Config);
		const self = this;

		

		self.on('close', (removed, done) => {
			done();
		});
	}

	RED.nodes.registerType('barred-config', Config);
};
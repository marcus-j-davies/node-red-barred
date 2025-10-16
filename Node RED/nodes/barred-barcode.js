module.exports = function (RED) {
	function Barcode(Config) {
		RED.nodes.createNode(this, Config);
		const self = this;

        self.config = Config;
		self.stack = RED.nodes.getNode(self.config.stack);

        self.stack.registerBarcodeEmitter(self.id,(msg) =>{
            self.send(msg);
        })

		self.on('close', (_, done) => {
			done();
		});
	}

	RED.nodes.registerType('barred-barcode', Barcode);
};
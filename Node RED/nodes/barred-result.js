module.exports = function (RED) {
	function BarredResult(config) {
		RED.nodes.createNode(this, config);
		const self = this;

		self.config = config;

		self.on('input', (msg, send, done) => {
			if (msg._barredCB) {
				if (msg._barredCB.expires > new Date().getTime()) {
					msg._barredCB.callback({
						status: msg.status || config.defaultStatus,
						payload: msg.payload,
						payloadType: typeof msg.payload
					});
				} else {
					done(new Error('The BARRED response object has expired.'));
					return;
				}

				delete msg._barredCB;
				send(msg);
				done();
			} else {
				done(new Error('No BARRED Callback object found in the message, did you re-declare the `msg` object?'));
			}
		});

		self.on('close', (_, done) => {
			done();
		});
	}

	RED.nodes.registerType('barred-result', BarredResult);
};

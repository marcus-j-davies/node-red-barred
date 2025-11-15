module.exports = function (RED) {
	function BarredResult(config) {
		RED.nodes.createNode(this, config);
		const self = this;

		self.config = config;

		self.on('input', (msg, send, done) => {
			if (msg._barredCB) {
				if (msg._barredCB.expires > new Date().getTime()) {
					if (msg.status === 'MENU') {
						Object.values(msg.payload).forEach((MI) => {
							if (MI.context) {
								MI.contextType = typeof MI.context;
							}
						});
					}
					msg._barredCB.callback({
						title: msg.topic,
						status: msg.status || config.defaultStatus,
						payload: msg.payload,
						attachment: msg.status === 'OK' && msg.attachment ? msg.attachment : undefined,
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

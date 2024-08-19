let socket;
mergeInto(LibraryManager.library, {
	configSocket: function (url) {
		socket = io(UTF8ToString(url));

		socket.on('connect', function () {
			myGameInstance.SendMessage('NetworkManager', 'OnConnected', '');
		});
		socket.on('disconnect', function () {
			myGameInstance.SendMessage('NetworkManager', 'OnDisconnected', '');
		});

		socket.on('init', function (response) {
			myGameInstance.SendMessage('NetworkManager', 'OnInit', '');
		});
		socket.on('numberParticipants', function (response) {
			myGameInstance.SendMessage(
				'NetworkManager',
				'OnNumberParticipants',
				response.toString()
			);
		});
		socket.on('isStarted', function (response) {
			myGameInstance.SendMessage('NetworkManager', 'OnIsStarted', response.toString());
		});
		socket.on('admin', function (response) {
			myGameInstance.SendMessage('NetworkManager', 'OnAdmin', '');
		});
		socket.on('leftTime', function (response) {
			myGameInstance.SendMessage('NetworkManager', 'OnTimeLeft', response.toString());
		});
		socket.on('endGame', function (response) {
			myGameInstance.SendMessage(
				'NetworkManager',
				'OnEndGame',
				JSON.stringify(response)
			);
		});
		socket.on('getAllPlayers', function (response) {
			myGameInstance.SendMessage(
				'NetworkManager',
				'OnGetAllPlayers',
				JSON.stringify(response)
			);
		});
		socket.on('createMap', function (response) {
			myGameInstance.SendMessage(
				'NetworkManager',
				'OnCreateMap',
				JSON.stringify(response)
			);
		});
		socket.on('joinGame', function (response) {
			myGameInstance.SendMessage(
				'NetworkManager',
				'OnJoinGame',
				JSON.stringify(response)
			);
		});
		socket.on('disconnectPlayer', function (response) {
			myGameInstance.SendMessage(
				'NetworkManager',
				'OnDisconnectPlayer',
				JSON.stringify(response)
			);
		});
		socket.on('addPlayer', function (response) {
			myGameInstance.SendMessage(
				'NetworkManager',
				'OnAddPlayer',
				JSON.stringify(response)
			);
		});
		socket.on('moves', function (response) {
			myGameInstance.SendMessage(
				'NetworkManager',
				'OnMovePlayer',
				JSON.stringify(response)
			);
		});
		socket.on('rotates', function (response) {
			myGameInstance.SendMessage(
				'NetworkManager',
				'OnRotatePlayer',
				JSON.stringify(response)
			);
		});
		socket.on('shoot', function (response) {
			myGameInstance.SendMessage('NetworkManager', 'OnShoot', JSON.stringify(response));
		});
		socket.on('hit', function (response) {
			myGameInstance.SendMessage('NetworkManager', 'OnHit', JSON.stringify(response));
		});
		socket.on('throwGrenade', function (response) {
			myGameInstance.SendMessage(
				'NetworkManager',
				'OnThrowGrenade',
				JSON.stringify(response)
			);
		});
	},

	hit: function (playerDTO) {
		socket.emit('hit', UTF8ToString(playerDTO));
	},
	init: function () {
		socket.emit('init');
	},
	rotates: function (playerDTO) {
		socket.emit('rotates', UTF8ToString(playerDTO));
	},
	shoot: function (playerDTO) {
		socket.emit('shoot', UTF8ToString(playerDTO));
	},
	throwGrenade: function (playerDTO) {
		socket.emit('throwGrenade', UTF8ToString(playerDTO));
	},
	moves: function (playerDTO) {
		socket.emit('moves', UTF8ToString(playerDTO));
	},
	endGame: function (playerDTO) {
		socket.emit('endGame', UTF8ToString(playerDTO));
	},
	joinGame: function (playerDTO) {
		socket.emit('joinGame', UTF8ToString(playerDTO));
	},
	disconnectPlayer: function (playerDTO) {
		socket.emit('disconnectPlayer', UTF8ToString(playerDTO));
	},
	resetGameIO: function () {
		socket.emit('resetGame');
	},
});

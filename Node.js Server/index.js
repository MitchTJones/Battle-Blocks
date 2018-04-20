var express = require('express');
app = express();
http = require('http');
server = http.createServer(app);
io = require('socket.io').listen(server);

server.listen(3000);

app.get('/', function (req, res) {
  res.sendFile(__dirname + '/client/index.html');
});
app.use('/client', express.static(__dirname + '/client'));

var mainWindow = null;

var minPlayers = 2;
var maxPlayers = 2;

var player1;
var player2;

var connections = 0;
var sockets = [];
var players = [];
var spectators = [];

var lobbyCountdown = {i:{}, n:'lobby',o:3,t:3};

var gamestartCountdown = {i:{}, n:'gamestart',o:3,t:3};

var turnCountdown = {i:{}, n:'turn',o:30,t:30};

var devMode = false;

var round = 1;

console.log('Server Started');

io.on('connection', function(socket) {
	socket.number = connections;
	console.log(socket.number + ': new client connected');
	connections++;
	initSocket(socket);

	socket.on('signin', function(data) {
		if (players.length >= minPlayers) {
			initSpectator(socket, data);
		} else {
			initPlayer(socket, data);
		}
		if (players.length == minPlayers) {
			console.log('sending ' + socket.username + ' info about ' + socket.enemy.username);
			socket.emit('addplayer', socket.enemy.player);
			console.log('sending ' + socket.enemy.username + ' info about ' + socket.username);
			socket.enemy.emit('addplayer', socket.player);
		}
	});

	socket.on('ready', function() {
		socket.ready = true;
		if (players.length >= minPlayers) {
			socket.enemy.emit('enemyready');
			if (check(players, 'ready')) {
				console.log('   all players ready - starting lobby countdown');
				player1 = players[0];
				console.log('   ' + player1.username + ' is now player 1');
				player1.emit('player', {p:1, e:2});
				player2 = players[1];
				console.log('   ' + player2.username + ' is now player 2');
				player2.emit('player', {p:2, e:1});
				lobbyCountdown.i = setTimer(lobbyCountdown, 'loadgame');
			}
		}
	});

	socket.on('unready', function() {
		socket.ready = false;
		socket.broadcast.emit('enemyunready');
		endTimer(lobbyCountdown);
	});

	socket.on('gameloaded', function() {
		console.log(socket.number + ': game scene has loaded, sending game start information')
		socket.gameLoaded = true;
		socket.emit('newround');
		socket.emit('newscore', {p1:0, p2:0});
		if (check(players, 'gameLoaded')) {
			gamestartCountdown.i = setTimer(gamestartCountdown, function() {
				emitTo(players, 'startturn')
				turnCountdown.i = setTimer(turnCountdown, 'endturn');
			});
		}
	});

	socket.on('turn', function(data) {
		console.log(socket.number + ': received turn');
		socket.turnEnded = true;
		socket.turn = data;
		socket.turns.push(data);
		if (check(players, 'turnEnded')) {
			resetTimer(turnCountdown);
			setAll(players, 'turnEnded', false);
			console.log('   all turns received - sending turns');
			socket.emit('enemyturn', {turn:socket.enemy.turn});
			socket.enemy.emit('enemyturn', {turn:socket.turn});
		}
	});

	socket.on('requestnextturn', function(data) {
		socket.nextTurn = true;
		if (check(players, 'nextTurn')) {
			round++;
			for (i = 0; i < players.length; i++) {
				players[i].turns.push(players[i].turn);
			}
			emitTo(sockets, 'round', round);
			turnCountdown.i = setTimer(turnCountdown, 'endturn');
			emitTo(players, 'startturn');
			setAll(players, 'nextTurn', false);
		}
	});

	socket.on('disconnect', function() {
		var reg = false;
		updateIds();
		try {
			if (socket.registered) {
				reg = true;
			}
		} catch(e) {
			console.log('unregistered client disconnected');
		}
		if (reg) {
			console.log(socket.number + ': player ' + socket.username + ' has disconnected');
			sockets.pop(socket.socketId);
			if (socket.type == 'player') {
				players.pop(socket.playerId);
				for (i = 0; i < players.length; i++) {
					if (socket == players[i].enemy) {
						players[i].enemy = undefined;
					}
				}
			} else if (socket.type == 'spectator') {
				spectators.pop(socket.spectatorId);
			}
			if (socket == player1) {
				player1 = undefined;
			}
			if (socket == player2) {
				player2 = undefined;
			}
			emitTo(players, 'remove' + socket.type, {username: socket.username});
			console.log('   current Socket Count: ' + sockets.length);
			if (players.length < minPlayers)
				emitTo(players, 'endgame', {message:'Not enough players'});
		}
	});

	socket.on('enemyhit', function() {
		// console.log(socket.number + ': player ' + socket.username + ' claims to have hit ' + socket.enemy.username);
		socket.hitEnemy = true;
		checkHit(socket.enemy);
	});

	socket.on('localhit', function() {
		// console.log(socket.number + ': player ' + socket.username + ' claims to have been hit by ' + socket.enemy.username);
		socket.gotHit = true;
		checkHit(socket);
	});

	socket.on('hello', function() {
		console.log('hello');
	});
});

function simplifyPlayers(oldArr) {
	var newArr;
	for (i = 0; i < oldArr.length; i++) {
		var o = oldArr[i];
		newArr[i] = {username:o.username, ready:o.ready};
	}
	return newArr;
}

function processTurn(turn) {
	var playerArr = turn.turn;
	var projectileArr = [];
	var projectileOrigin = {x:NaN, y:NaN};
	if (turn.shot.shot) {
		for (i = 0; i < playerArr.length; i++) {
			if (i == turn.shot.f) {
				projectileOrigin = playerArr[i];
				projectileArr[i] = projectileOrigin;
			} else if (i > turn.shot.f) {
				var diff = i - turn.shot.f;
				projectileArr[i] = addVectors(projectileOrigin, multVectors(turn.shot.s, diff/60));
			} else {
				projectileArr[i] = {x:NaN, y:NaN};
			}
		}	
	} else {
		for (i = 0; i < playerArr.length; i++) {
			projectileArr[i] = {x:NaN, y:NaN};
		}
	}
	return {player: playerArr, shot: projectileArr};
}

function initSocket(socket) {
	socket.registered = true;
	sockets.push(socket);
	updateIds();
	socket.emit('verification');
}

function initPlayer(socket, data) {
	console.log(socket.number + ': initializing as player');
	socket.username = data.username;
	socket.type = 'player';
	socket.ready = false;
	socket.gameLoaded = false;
	socket.turnEnded = false;
	socket.score = 0;
	socket.turns = [];
	players.push(socket);
	updateIds();
	socket.player = data;
	for (p = 0; p < players.length; p++) {
		if (p != socket.playerId) {
			socket.enemy = players[p];
			socket.enemy.enemy = socket;
			console.log(socket.username + ' (' + socket.playerId + ') and ' + socket.enemy.username + ' (' + socket.enemy.playerId + ') are now enemies');
		}
	}
	socket.emit('signin-response');
	console.log(socket.number + ': registered ' + socket.username + ' as player.');
}

function initSpectator(socket, data) {
	socket.username = data.username;
	socket.type = 'spectator';
	spectators.push(socket);
	updateIds();
	socket.emit('signin-spectator');
}

function setTimer(timer, message) {
	console.log('   Timer ' + timer.n + ' has been set');
	var interval = setInterval(function() {
		// console.log('   Timer ' + timer.n + ': ' + timer.t);
		emitTo(players, 'countdown', {name: timer.n, time: timer.t});
		timer.t--;
		if (timer.t < 1) {
			if (typeof message == 'string')
				emitTo(sockets, message);
			if (typeof message == 'function')
				message();
			resetTimer(timer);
		}
	}, 1000);
	return interval;
}

function endTimer(timer) {
	emitTo(players, 'cancelcountdown', {name:timer.n})
	resetTimer(timer);
}

function resetTimer(timer) {
	clearInterval(timer.i);
	timer.t = timer.o;
}

function check(array, bool) {
	var ready = true;
	for (i = 0; i < array.length; i++) {
		if (!array[i][bool]) {
			ready = false;
		}
	}
	return ready;
}

function checkHit(socket) {
	if (socket.gotHit) {
		if (socket.enemy.hitEnemy) {
			console.log('   confirmed: player ' + socket.enemy.username + ' has hit ' + socket.username + '!');
			socket.enemy.score++;
			emitTo(sockets, 'newround');
			for (i = 0; i < players.length; i++) {
				players[i].emit('setscore', {p1:players[i].score, p2:players[i].enemy.score});
			}
		}
	}
}

function emitTo(array, name, data) {
	for (i = 0; i < array.length; i++) {
		if (data == undefined)
			array[i].emit(name);
		else {
			if (typeof data == 'object')
				array[i].emit(name, data);
			else
				array[i].emit(name, {data:data});
		}
	}
}

function updateIds() {
	for (i = 0; i < sockets.length; i++) {
		sockets[i].socketId = i;
	}
	for (i = 0; i < players.length; i++) {
		players[i].playerId = i;
	}
	for (i = 0; i < spectators.length; i++) {
		spectators[i].spectatorId = i;
	}
}

function setAll(array, name, data) {
	for (i = 0; i < array.length; i++) {
		array[i][name] = data;
	}
}

function positions(arr) {
	var output = [];
	for (i = 0; i < arr.length; i++) {
		output[i] = arr[i].position;
	}
	return output;
}

function vectorNaN(v) {
	return (isNaN(v.x) || isNaN(v.y));
}

function addVectors(v1, v2) {
	return {x: v1.x+v2.x, y: v1.y + v2.y};
}

function multVectors(v1, v2) {
	if (typeof v2 == 'number')
		return {x: v1.x*v2, y: v1.y*v2};
	else
		return {x: v1.x*v2.x, y: v1.y*v2.y};
}

// process.on('uncaughtException', function(err) {
// 	console.log('Oh no!  ' + err);
// });
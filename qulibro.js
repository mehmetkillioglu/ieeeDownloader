(function(ext) {
 	console.log("Loading Acrome Qulibro extension version 1.2...");

    var connected = false;
	var position = [0,0];
	var position_x = 0;
	var position_y = 0;
	var des_position = [0,0];
	var sensor_values = [0,0,0,0];
	var ps4_buttons = [0,0,0,0,0,0,0,0];
	var ps4_coordinates = [0,0,0,0,0,0,0,0];
	var messagestring = 0;
    var myStatus = 1; // initially yellow
    var myMsg = 'not_ready';

    //ext.cnct = function (callback) {
    ext.cnct = function () {
        window.socket = new WebSocket("ws://127.0.0.1:9000");
        window.socket.binaryType = 'blob';
        window.socket.onopen = function () {
            var msg = JSON.stringify({
                "command": "scratch_ready"
            });
            window.socket.send(msg);
            myStatus = 2;

            // change status light from yellow to green
            myMsg = 'ready';
            connected = true;
        };

        window.socket.onmessage = function (message) {
            //alert(message.data);
            //var asciiVal = hex2a(message.data);
            //alert(asciiVal);
            //var msg = JSON.parse(asciiVal);
            //apllert(msg);
            var tmpmsg = message.data.replace(/'/g, '"');

            var msg = JSON.parse(tmpmsg);

            //alert(msg);
            // handle the only reporter message from the server
            // for changes in digital input state
            var reporter = msg['report'];
            if(reporter === 'ball_position') {
                //var x_pos = msg['x'];
                //var y_pos = msg['y'];
                position_x = Number(msg['x']);
                position_y = Number(msg['y']);
                sensor_values[0] = Number(msg['A0']);
                sensor_values[1] = Number(msg['A1']);
                sensor_values[2] = Number(msg['A2']);
                sensor_values[3] = Number(msg['A3']);
                //position_x = parseInt(msg['x']);
            }
            if(reporter === 'ps4_data'){
                ps4_coordinates[0] = Number(msg['l_s_x']);
                ps4_coordinates[1] = Number(msg['l_s_y']);
                ps4_coordinates[2] = Number(msg['r_s_x']);
                ps4_coordinates[3] = Number(msg['r_s_y']);
                ps4_coordinates[4] = Number(msg['l_2']);
                ps4_coordinates[5] = Number(msg['r_2']);
                ps4_coordinates[6] = Number(msg['a_x']);
                ps4_coordinates[7] = Number(msg['a_y']);
                ps4_buttons[0] = Number(msg['squ']);
                ps4_buttons[1] = Number(msg['xbt']);
                ps4_buttons[2] = Number(msg['crc']);
                ps4_buttons[3] = Number(msg['tri']);
                ps4_buttons[4] = Number(msg['l_1']);
                ps4_buttons[5] = Number(msg['r_1']);
                ps4_buttons[6] = Number(msg['psb']);
                ps4_buttons[7] = Number(msg['tpb']);
            }

            console.log(message.data);
        };
        window.socket.onclose = function (e) {
            console.log("Connection closed.");
            socket = null;
            connected = false;
            myStatus = 1;
            myMsg = 'not_ready';
        };
    };
ext._shutdown = function () {
        var msg = JSON.stringify({
            "command": "scratch_shutdown"
        });
        window.socket.send(msg);
    };
    // Status reporting code
    // Use this to report missing hardware, plugin or unsupported browser
//    ext._getStatus = function()  {
//		if (ws.readyState == 1) {
//			return {status: 2, msg: 'Baglandi'};
//		}else	{return {status: 1, msg: 'Baglanti kurulamadi'};}
//	};
    ext._getStatus = function (status, msg) {
        return {status: myStatus, msg: myMsg};
    };
	ext.getBallPosition = function(direction) {
        if (direction === 'x'){
            return position_x;
        } else if (direction ==='y'){
            return position_y;
        }
	};
	ext.getSensorData = function(channel){
	    switch(channel){
            case 'A0':
                return sensor_values[0];
                break;
            case 'A1':
                return sensor_values[1];
                break;
            case 'A2':
                return sensor_values[2];
                break;
            case 'A3':
                return sensor_values[3];
                break;
            default:
                return 0;
	    }
	};
	ext.getCoordinateData = function(option){
	//controllerTags: ['Sol Çubuk X','Sol Çubuk Y','Sağ Çubuk X','Sağ Çubuk Y','L2','R2','Yön X','Yön Y'],
	    switch(option){
            case 'Sol Çubuk X':
                return ps4_coordinates[0];
                break;
            case 'Sol Çubuk Y':
                return ps4_coordinates[1];
                break;
            case 'Sağ Çubuk X':
                return ps4_coordinates[2];
                break;
            case 'Sağ Çubuk Y':
                return ps4_coordinates[3];
                break;
            case 'L2':
                return ps4_coordinates[4];
                break;
            case 'R2':
                return ps4_coordinates[5];
                break;
            case 'Yön X':
                return ps4_coordinates[6];
                break;
            case 'Yön Y':
                return ps4_coordinates[7];
                break;
            default:
                return 0;
	    }
	};
	ext.getButtonData = function(option){
	//buttonTags: ['Kare','Çarpı','Çember','Üçgen','L1','R1','PS Tuşu','Dokunmatik']
		switch(option){
            case 'Kare':
                return ps4_buttons[0];
                break;
            case 'Çarpı':
                return ps4_buttons[1];
                break;
            case 'Çember':
                return ps4_buttons[2];
                break;
            case 'Üçgen':
                return ps4_buttons[3];
                break;
            case 'L1':
                return ps4_buttons[4];
                break;
            case 'R1':
                return ps4_buttons[5];
                break;
            case 'PS Tuşu':
                return ps4_buttons[6];
                break;
            case 'Dokunmatik':
                return ps4_buttons[7];
                break;
            default:
                return 0;
	    }

	}
	ext.setDesiredPosition = function(x_position,y_position){
	    if (x_position<-172){x_position = -172;}
	    if (x_position>172){x_position = 172;}
	    if (y_position<-136){y_position = -136;}
	    if (y_position>136){y_position = 136;}

        var msg = JSON.stringify({
            "command": 'desired_position', 'x': x_position, 'y': y_position
        });
        console.log(msg);
        window.socket.send(msg);
	};

	ext.setServoPosition = function(x_position,y_position){
	    if (x_position<-60){x_position = -60;}
	    if (x_position>60){x_position = 60;}
	    if (y_position<-60){y_position = -60;}
	    if (y_position>60){y_position = 60;}

        var msg = JSON.stringify({
            "command": 'servo_position', 'x_servo': x_position, 'y_servo': y_position
        });
        console.log(msg);
        window.socket.send(msg);
	};
	ext.startControl = function(){
        var msg = JSON.stringify({
            "command": 'start_control'
        });
        console.log(msg);
        window.socket.send(msg);
	};
	ext.stopControl = function(){
        var msg = JSON.stringify({
            "command": 'stop_control'
        });
        console.log(msg);
        window.socket.send(msg);
	};
	ext.goCenter = function(){
        var msg = JSON.stringify({
            "command": 'center_ball'
        });
        console.log(msg);
        window.socket.send(msg);
	};

	ext.Disconnect = function (callback) {
		/*if (!(window_socket === null)) {
			if (window_socket.readyState == 1) {

				console.log("Baglanti kesiliyor");
				window_socket.close();
			} else { console.log ("Baglanti zaten kesik");}
		}*/
        var msg = JSON.stringify({
            "command": "scratch_shutdown"
        });
        window.socket.send(msg);
        if (!(window.socket === null)){

			if (window.socket.readyState == 1) {

				console.log("Baglanti kesiliyor");
				window.socket.close();
			} else { console.log ("Baglanti zaten kesik");}
        }
	};
    // Block and block menu descriptions
    var descriptor = {
        blocks: [
            // Block type, block name, function name, param1 default value, param2 default value
            //['r', 'get data from %m.anaChannels', 'GetData', "Channel 0"],
            //[' ', 'turn %m.anaChannels  %m.onOff', 'SetData', "Channel 7", "off"],
			['r', 'Top pozisyonu %m.posTags', 'getBallPosition', "x"],
			['r', 'Sensor değeri %m.sensorTags', 'getSensorData', "A0"],
			['r', 'PS4 Yön Tuşu %m.coordinateTags', 'getCoordinateData', "Sol Çubuk X"],
			['r', 'PS4 Buton %m.buttonTags', 'getButtonData', "Kare"],
			//['r', 'Top pozisyonu blabala', 'getBallPosition', "x"],
			//['r', 'gelendata', 'gelendata'],
			[' ', 'x =  %n y = %n noktasına git', 'setDesiredPosition', "0","0"],
			[' ', 'Servo X =  %n Servo Y = %n döndür', 'setServoPosition', "0","0"],
			[' ', 'Kontrolu başlat','startControl'],
			[' ', 'Kontrolu durdur','stopControl'],
			[' ', 'Merkeze git','goCenter'],
			//['b', 'Konuma ulasildiginda','whenArrived'],
			//Connection
			[' ', "Qulibro'ya baglan", 'cnct'],
			[' ', 'Bağlantıyı kes', 'Disconnect']
        ],
		
		menus: {
			//anaChannels: ['Channel 0', 'Channel 1', 'Channel 2', 'Channel 3', 'Channel 4', 'Channel 5', 'Channel 6', 'Channel 7'],
			//onOff: ['on', 'off']
			posTags: ['x','y'],
			sensorTags: ['A0','A1','A2','A3'],
			coordinateTags: ['Sol Çubuk X','Sol Çubuk Y','Sağ Çubuk X','Sağ Çubuk Y','L2','R2','Yön X','Yön Y'],
			buttonTags: ['Kare','Çarpı','Çember','Üçgen','L1','R1','PS Tuşu','Dokunmatik']
		}
    };

    // Register the extension
    ScratchExtensions.register('Qulibro', descriptor, ext);
})({});

var express = require('express');
var bodyParser = require('body-parser');
var multer  = require('multer');
var fs  = require('fs');
var path = require('path');

var crypto = require('crypto');
var keygen = require('rsa-keygen');
var ursa = require('ursa');

var key = ursa.createPrivateKey(fs.readFileSync('key.pem'))
var pub = ursa.createPublicKey(fs.readFileSync('IIS.pub'));

//console.log(Object.getOwnPropertyNames(keys.private_key));
var app = express();
var app2 = express();
var app3 = express();

app.use(bodyParser.urlencoded({extended:false}));
app2.use(bodyParser.urlencoded({extended:false}));
app3.use(bodyParser.urlencoded({extended:false}));

app.use(multer());
app2.use(multer());
app3.use(multer());

function receive(req, res, next){

    var JSONobject = req.body;
    var uniqueToken = makeid();

    for (var i in JSONobject) {
        JSONobject[i] = key.decrypt(JSONobject[i], 'base64', 'utf8', ursa.RSA_PKCS1_PADDING);
            	console.log(JSONobject[i]);
    }
    for (var j in JSONobject) {
        JSONobject[j] = pub.encrypt(JSONobject[j], 'utf8', 'base64', ursa.RSA_PKCS1_PADDING);
            	console.log(JSONobject[j]);
    }
    JSONobject['uniqueToken'] = uniqueToken;
    res.end(JSON.stringify(JSONobject));	
}

function sendDocs(req, res, next){
   
    var filePath = path.join(__dirname, '../uploads');
    var newPath = path.join(__dirname, '../retrieved');
  
    var allFilenames = fs.readdirSync(filePath);
    var allPDFs = [];
    for (var i = 0; i < allFilenames.length; i++){
	if (allFilenames[i].split(".").pop() == "pdf"){
	    allPDFs.push(allFilenames[i]);
        }
    }  
    if (allPDFs.length > 0){
  
        var filename = allPDFs[0];     
  	var wholePath = path.join(filePath, filename);
	var wholeNewPath = path.join(newPath, filename);
        var extension = "pdf";
        var clientId = filename.substr(0, filename.indexOf('_'));        
        console.log(filename);
        var responseObj = new Object();
            responseObj['clientId'] = clientId;
            responseObj['documentName'] = filename;
            responseObj['documentExtension'] = extension;
            
        var stat = fs.statSync(wholePath);
        res.writeHead(200, {
            'Content-Type': 'multipart/formdata',
            'Content-Length': stat.size
    	});

        var readStream = fs.createReadStream(wholePath);

        readStream.pipe(res);

 
           fs.rename(wholePath, wholeNewPath);
           console.log(JSON.stringify(responseObj));
           res.end(JSON.stringify(responseObj));
    }
    else{
      console.log("nothing left");
      res.end();
    }
}

function makeid()
{
    var text = "";
    var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    for( var i=0; i < 5; i++ ){
        text += possible.charAt(Math.floor(Math.random() * possible.length));
    }

    return text;
}

function upload(req, res, next){
    var clientId = req.body['clientId'];
    var fileName = req.body['fileName'];
    console.log(fileName);
     // get the temporary location of the file
    var tmp_path = req.files.file.path;
    console.log(tmp_path);
    // set where the file should actually exists - in this case it is in the "images" directory
    var target_path = '../uploads/' + clientId + "_" + Date.now() +  "_" +  fileName;
    // move the file from the temporary location to the intended location
    console.log(target_path);
    fs.rename(tmp_path, target_path, function(err) {
        if (err) throw err;
        // delete the temporary file, so that the explicitly set temporary upload dir does not get filled with unwanted files
        fs.unlink(tmp_path, function() {
            if (err) throw err;
            res.send('File uploaded to: ' + target_path + ' - ' + req.files.file.size + ' bytes');
        });
    });
}

app.post('/', function(req, res, next){
    if (req.body['firstname']){
        receive(req, res, next);
    }
    else if(req.body['update']){
        sendDocs(req, res, next); 	
    }
    else{
        upload(req, res, next);
    }
});
app2.post('/', function(req, res, next){
     if (req.body['firstname']){
        receive(req, res, next);
    }
    else if(req.body['update']){
        sendDocs(req, res, next); 	
    }
    else{
        upload(req, res, next);
    }
});
app3.post('/', function(req, res, next){
    if (req.body['firstname']){
        receive(req, res, next);
    }
    else if(req.body['update']){
        sendDocs(req, res, next); 	
    }
    else{
        upload(req, res, next);
    }
});
app.listen(9000, function(){
	console.log("Port 9000");
});
app2.listen(9001, function(){
        console.log("Port 9001");
});
app3.listen(9002, function(){
        console.log("Port 9002");
});



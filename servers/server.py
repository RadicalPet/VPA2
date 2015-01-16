#!/usr/bin/env python
from threading import Thread
from BaseHTTPServer import BaseHTTPRequestHandler, HTTPServer
from httplib import HTTPResponse
from SocketServer import ThreadingMixIn
import json
import urllib
import requests
import cgi
from Crypto.Cipher import PKCS1_v1_5
from Crypto.PublicKey import RSA
from Crypto.Hash import SHA
from Crypto import Random
from base64 import b64decode

pubKey = RSA.importKey(open('pyKey.pub').read())
pubCipher = PKCS1_v1_5.new(pubKey)
key = RSA.importKey(open('IIS.pem').read())
cipher = PKCS1_v1_5.new(key)

def decrypt_IIS(msgIn):
    dsize = SHA.digest_size
    sentinel = Random.new().read(15+dsize) 
    return cipher.decrypt(b64decode(msgIn), sentinel)
    
def encrypt_Py(msgOut):
    return pubCipher.encrypt(msgOut).encode('base64') 

class S(BaseHTTPRequestHandler):
    def _set_headers(self):
        self.send_response(200)
        self.send_header('Content-type', 'application/json')
        self.end_headers()
 
    def do_GET(self):
        self._set_headers()
        self.wfile.write("get")
 
    def do_HEAD(self):
        self._set_headers()

    def do_POST(self):
        length = int(self.headers.getheader('content-length'))
        data =  cgi.parse_qs(self.rfile.read(length), keep_blank_values=1)
        dataOut = {}
        
        if data.has_key('firstname'):
            firstNameDec = decrypt_IIS(data['firstname'][0])
	    lastNameDec = decrypt_IIS(data['lastname'][0])
            emailDec =  decrypt_IIS(data['email'][0])
 	    messageDec =  decrypt_IIS(data['message'][0])


            dataOut['firstname'] = encrypt_Py(firstNameDec)
            dataOut['lastname'] = encrypt_Py(lastNameDec)
            dataOut['email'] = encrypt_Py(emailDec)
            dataOut['message'] = encrypt_Py(messageDec)
	
        else:
            emailDec =  decrypt_IIS(data['email'][0])
            dataOut['email'] = encrypt_Py(emailDec)
        
        dataOutJSON = json.dumps(dataOut)    
        self.send_response(200)
        self.send_header("Content-type", "application/json")
        self.send_header("Content-Length", len(dataOutJSON))
        self.end_headers()
        self.wfile.write(dataOutJSON)
        
class ThreadingHTTPServer(ThreadingMixIn, HTTPServer):
    allow_reuse_address = True

def serve_on_port(port):
    server = ThreadingHTTPServer(("", port), S)
    print 'Running on port: ' + str(port)
    try:
        server.serve_forever()
    except (KeyboardInterrupt, SystemExit):
        server.server_close()

        
if __name__ == "__main__":
    from sys import argv


a = Thread(target=serve_on_port, args=[9000])
a.daemon = True
a.start()
b = Thread(target=serve_on_port, args=[9001])
b.daemon = True
b.start()
serve_on_port(9002)

    


# Venal Penal Action - a mock lawyer's website implementing openSSL public key crypto
 
School Project C#, .NET MVC and Networking. Exam Project concluding a module with 1 week of networking introduction with
Node.js and Ruby, 5 days of pure C# introduction and 5 days of MVC and .NET with entity framework.
Also concluding the first semester of a 3 semester BA in web development at Copenhagen School of design and Technology.
 
## Project Description ##

There are 3 servers, the IIS (C#) on Windows and python and node.js on Debian. 
Both, the python and node.js server are behind an HAproxy server and listen to 3 ports. HAproxy does a simple roundrobin loadbalancing between those 3 ports.

Each server (instance) contains its own private key and one of the other servers' public keys. The goal of this is to have the data in the database physically separeted from the private key to decrypt it, to provide proxies for the dataflow, have dedicated servers for different computing tasks and to keep the data in transfer as secure as possible.
The keypairs are generated via openSSL and are only 1024 bit RSA and use the near deprecated  PKCS#1v.1.5 padding.
Since it is only a proof of concept and not even security focused school project, this should be not too unsettling. I chose it mostly for library/package compatibility reasons, as well as slightly shorter processing times.

A new client can get in contact via the contact form, where they click a button for clientside encryption with the node servers public key. Via Restsharp (http POST request), the data gets the send to node as JSON object. On the node server it gets decrypted, encrypted with the python server's public key, a unique token is added in plaintext and the data is sent back to RestSharp as response. C# stores the encrypted data into the database, but sends the emailaddress on to the python server. Here it gets decrypted, encrypted with the IIS public key and sent back to C#. C# decrypts it and now sends one email to an employee, notifying about a new message/client and one email to the client, providing them with the unique token.

With that Token, the customer can upload image files or documents. Those get sent to node (unencrypted, due to lack of time :) ), together with the clientId, where they get renamed with a clientId_timestamp_filename.extension convention and moved into an /uploads directory.  An eventdriven scheduled Bash task, watches this directory for incoming files and converts them to pdf format. 

In the CMS (links bottom right in the footer, no password) if the employee clicks "Update" in the documents section, all Files in pdf format get downloaded into a /ClientDocs directory on Windows (IIS) and moved into a /received directory on Debian (node). The clientId, Filename and extension get stored in the database and displayed for the employee.

The client data gets displayed encrypted in the CMS with only token and clientId in plaintext. If the employee clicks "Decrypt" the data gets sent to python, decrypted, encrypted with the IIS pub key, decrypted by C# and displayed.

## Setup and Configuration
 
There are a couple of absolute Pathes in the Home Controller, for File Downloads and the IIS private key (the node public key is stored as modulus string in the JavaScript).

The smtp client connects to "smtp.gmail.com", so gmail credentials have to be provided.

Node can be run as service with 

		forever start server.js

after installling the forever module via npm.

The scheduled task in bash is done by incron and the command to convert into pdf is part of some libreoffice coreutils, that run as service in the background.

		incrontab -e
		/root/uploads IN_MOVED_TP unoconv /root/uploads/$#

The included haproxy.cfg file (usually located in /etc/haproxy/), has to be added to the respective IP address of where the servers are run or "" for localhost.
RestSharp also needs teh right ip addresses and/or path.

Python is run as multithreaded http server and I did not have the time to completely daemonize it (I had to daemonize the threads, to be able to keyboard interrupt and make the ports free again, but did not have the time to read into how to do that with the entire script), so it dies with its shell unfortunately.

In python and node the keys are in the same directory as the server. The /uploads and /received are one above node.

The keypairs are not included in the solution and can be generated on a unix based system with openSSL installed via
 
		openssl genrsa -out key.pem
 		openssl rsa -in mykey.pem -pubout > mykey.pub

The modulus string (different format of a public key) used in my JavaScript implementation via the jsbn (and related) JavaScript crypto libs (credited below) is achieved like:
 
		openssl rsa -in key.pem -noout -modulus

## Modules, packages, libraries and Credits

The used libraries, node modules, python and nuGET packages can of course be seen in the script / are included in the solution as far as C# goes.

Nonetheless do I need to credit:

		http://www-cs-students.stanford.edu/~tjw/jsbn/

for their comprehesive demo and straightforward JavaScript cryptolibraries, stackoverflow for providing the skeleton code for almost everything and Windows for not being even buggier and awkward than it is.
 
## License
 
 Most code only modified by me, so whatever the original code goes by, from my side:
 GPLv2


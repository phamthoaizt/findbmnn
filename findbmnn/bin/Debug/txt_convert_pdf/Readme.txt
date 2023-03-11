mandiant apatedns
apatedns is a tool for controlling dns responses though an easy to use gui. as a phony dns server,
apatedns spoofs dns responses to a user-specified ip address by listening on udp port 53 on the local
machine. it responds to dns requests with the response set to any ip address you specify. the tool logs
and timestamps any dns request it receives. you may specify a number of non-existent domain
(nxdomain) responses to send before returning a valid response.  apatedns also automatically sets
the local dns to localhost. by default, it will use either the set dns or default gateway settings as an ip
address to use for dns responses. upon exiting the tool, it sets back the original local dns settings.

usage
 after running the tool, you are presented with the following options:
o dns reply ip – this is the ip address that will be provided in dns responses.
o # of nxdomain’s – this is the number of non-existent domain responses to send before
responding with a valid response for each dns query.
o selected interface – this is the interface on which the dns server will listen.
o start server button – this is used to start the dns server.  this button must be pressed
before apatedns will process any dns requests.
o stop server button – this is used to stop the dns server after is has been started.
 a malware analyst may wish to use the tool in the following ways:
o to catch dns requests made by malicious software.
o to trick the malware to send its malicious traffic to a host that the malware analyst
controls and monitors.
o to catch additional domains used by a malware sample through the use of the non-
existent domain (nxdomain) option. malware will often loop through the different
domains it has stored if the first or second domains are not found. using this
nxdomain option can trick malware into giving you additional domains it has in its
configuration.

system requirements
windows xp or greater
microsoft .net runtime >= 2.0

credits
apatedns was created by steve davis

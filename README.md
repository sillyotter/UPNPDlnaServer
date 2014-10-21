UPNP
====

A simple UPNP server in C# for .net/mono.

A while back I needed a UPNP/DLNA server that could run on a linux media
server and talk to a PS3.  I created this app in part by looking at the Java
PS3MediaServer and a lot of network protocol reverse engineering, as well as
looking at the DLNA spec.  I know it has some problems talking to other
clients, but I never cared enough to fix it.

It relies on lighttpd to do the actual file serving as I had some issues
getting the old mono runtime to do this with out stuttering.  This app then is
responsible for the notification over UDP of the servers presence, and of
handling the soap queries for listings, metadata, etc.  The lighttpd server
was then responsible for the efficient distribution of the video content.  

It worked quite nicely, but I stopped using the PS3 as a media player and
switched to a PLEX server talking to a Roku, so this has sat for a while
unused.  With a bit of work I'm sure it could be made more generally useful.

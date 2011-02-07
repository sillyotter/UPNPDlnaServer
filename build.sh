#!/bin/bash

# to do: check on dir creates, only configure/rebild lightppd if needed
# add a -aot pass on exe and dll first
# do lots of cool scripty things in here to make it do the right thing
# and print out messages saying what that is

mkdir bin

cd support
tar xzf lighttpd-1.4.28.tar.gz
cd lighttpd-1.4.28
mkdir build
./configure --prefix=`pwd`/build > /dev/null
make install > /dev/null

cd ../..

dmcs -recurse:src/MediaServer/*.cs \
	-reference:System.Xml.Linq.dll,Mono.Posix.dll,taglib-sharp.dll \
	-lib:lib -out:bin/MediaServer.exe -optimize -target:exe 

cp lib/taglib-sharp.dll bin
cp src/lighttpd.conf.tmpl bin
cp src/MediaServer/Configuration.xml bin
mkdir bin/MediaServer
cp -R src/MediaServer/Resources bin/MediaServer
mkdir bin/lighttpd
cp -R support/lighttpd-1.4.28/build/* bin/lighttpd


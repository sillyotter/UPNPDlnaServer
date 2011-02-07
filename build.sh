#!/bin/bash

# to do: check on dir creates, only configure/rebild lightppd if needed
# add a -aot pass on exe and dll first
# do lots of cool scripty things in here to make it do the right thing
# and print out messages saying what that is

function test_and_create_directory() {
	if [ ! -d $1 ]
	then
		mkdir $1
	fi
}

function clean() {
	rm -rf support/lighttpd-1.4.28
	rm -rf bin
	find . -type f -name '*~' -exec rm {} \;
}

function build() {

	test_and_create_directory bin


	if [ ! -e bin/lighttpd ]
	then
		if [ ! -e support/lighttpd-1.4.28 ]
		then
			cd support
			tar xzf lighttpd-1.4.28.tar.gz
			cd lighttpd-1.4.28
			mkdir build
			./configure --prefix=`pwd`/build > /dev/null 2>&1
			make install > /dev/null 2>&1
			cd ../..
		else
			mkdir /lighttpd
			cp -R support/lighttpd-1.4.28/build/* bin/lighttpd
		fi
	fi


	dmcs -recurse:src/MediaServer/*.cs \
		-reference:System.Xml.Linq.dll,Mono.Posix.dll,taglib-sharp.dll \
		-lib:lib -out:bin/MediaServer.exe -optimize -target:exe 

	cp lib/taglib-sharp.dll bin
	cp src/lighttpd.conf.tmpl bin
	cp src/MediaServer/Configuration.xml bin

	test_and_create_directory bin/MediaServer
	cp -R src/MediaServer/Resources bin/MediaServer

	test_and_create_directory bin/lighttpd
	cp -R support/lighttpd-1.4.28/build/* bin/lighttpd

	cd bin
	mono --aot --optimize=all *.exe *.dll > /dev/null 2>&1
	cd ..
}

case $1 in 
	clean) clean ;;
	*) build ;;
esac


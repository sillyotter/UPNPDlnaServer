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
	echo "Cleaning..."
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
			echo "Building lighttpd..."
			cd support
			tar xzf lighttpd-1.4.29.tar.gz
			cd lighttpd-1.4.29
			mkdir build
			./configure --prefix=`pwd`/build > /dev/null 2>&1
			make install > /dev/null 2>&1
			cd ../..
		fi

		mkdir bin/lighttpd
		cp -R support/lighttpd-1.4.29/build/* bin/lighttpd
	fi


	echo "Compiling MediaServer..."

	dmcs -recurse:src/MediaServer/*.cs \
		-reference:System.Web.dll,System.Xml.Linq.dll,Mono.Posix.dll,taglib-sharp.dll \
		-lib:lib -out:bin/MediaServer.exe -optimize -target:exe 

	echo "Deploying..."

	cp src/start.sh bin
	cp lib/taglib-sharp.dll bin
	cp src/lighttpd.conf.tmpl bin
	cp src/MediaServer/Configuration.xml bin

	test_and_create_directory bin/MediaServer
	cp -R src/MediaServer/Resources bin/MediaServer

	echo "AOT optimization..."
	cd bin
	mono --aot --optimize=all *.exe *.dll > /dev/null 2>&1
	cd ..
}

function run() {
	
	cd bin
	mono --optimize=all MediaServer.exe Configuration.xml

}

function deploy() {
	stop MediaServer
	cp -R bin/* /opt/MediaServer/
	start MediaServer
}

case $1 in 
	clean) clean ;;
	run) run ;;
	deploy) deploy ;;
	*) build ;;
esac


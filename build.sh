gmcs -optimize+ -target:library \
	-out:SSDP.dll -recurse:SSDP/*.cs \
	-reference:System.Xml.Linq.dll

gmcs -optimize+ -target:exe \
	-out:MediaServer.exe -recurse:MediaServer/*.cs \
	-reference:System.Xml.Linq.dll \
	-reference:System.Web.dll \
	-reference:Mono.Posix.dll \
	-reference:FlickrNet.dll \
	-reference:taglib-sharp.dll \
	-reference:Google.GData.Client.dll \
	-reference:Google.GData.Extensions.dll \
	-reference:Google.GData.Photos.dll \
	-reference:Google.GData.YouTube.dll \
	-reference:SSDP.dll

gmcs -optimize+ -target:exe \
	-out:MediaBrowser.exe -recurse:MediaBrowser/*.cs \
	-reference:System.Xml.Linq.dll \
	-reference:SSDP.dll

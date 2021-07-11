# CleanPNG
Cleans out noise from alpha channel.

To use drag a folder of images or some images onto the CleanPNG.exe. Then it'll ask the threshold for how much also to exclude. Value must be between 0-255. 254 clears out anything that isn't fully opaque. This will set the color to black and fully transparent. The Larger the images the slower this will be as it will loop though each pixel and check the alpha value.

This will save over the original images so back them up.

This project uses [Monogame 3.7.1](https://github.com/MonoGame/MonoGame/releases/tag/v3.7.1)

To build on linux:
```sh
sudo apt update
sudo apt-get --assume-yes install nuget mono-complete mono-devel gtk-sharp3
echo ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true | sudo debconf-set-selections
sudo apt-get --assume-yes install ttf-mscorefonts-installer
wget https://github.com/MonoGame/MonoGame/releases/download/v3.7.1/monogame-sdk.run
chmod +x monogame-sdk.run
sudo ./monogame-sdk.run --noexec --keep --target ./monogame
cd monogame
echo Y | sudo ./postinstall.sh
cd ..  
nuget restore
msbuild
```

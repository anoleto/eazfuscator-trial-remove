# eazfuscator-trial-remove
eazfuscator trial remove is a simple program that i made when im drunk, basically it removes the 3 instruction (call, brtrue.s, ret) or trial check call from the called "method" or entry point.

## Getting started
clone the repo

```sh
git clone https://github.com/anoleto/eazfuscator-trial-remove.git
```

build the project on visual studio or dotnet build on cli. _or just go to [release](https://github.com/anoleto/eazfuscator-trial-remove/releases) tab to download and use it._ <br/>

drag the assembly to the executeable or use cmd and run
```sh
eazfuscator-trial-remove.exe path/to/assembly
```
or you can use --dnlib arg to use dnlib library instead
```sh
eazfuscator-trial-remove.exe path/to/assembly --dnlib
```
you can use this to multiple assemblies
```sh
eazfuscator-trial-remove.exe assembly assembly2 --dnlib
```
### Requirements
.NET Framework 4.7.2 or higher <br/>
Windows OS

### Todo
- to make this useable on dlls
- more cleanup
# AdaptiveWrapPanel
WPF custom layout panel designed at first for use in [Aurora](https://github.com/antonpup/Aurora) project.
But I believe it can be useful in other projects too.

* This layout behaves like standard wrap panel but with some significant improvements. 
* Panel tries to fit children in columns and fit them to width. 
* At first it tries to give as much space to children as it can. 
* Then it composes some of children to one column, stops stretching stretchable children, and finally overflow columns in bottom direction.
* Panel supports different column break preference, horizontal and vertical children alignment, content alignment, column definitions.

# Demo
For better understanding, how it works and does it suits your you can run demo from [here](https://github.com/VoronFX/AdaptiveWrapPanel/releases/) or build sources by your own.

# Installation
You can install it by [NuGet package](https://www.nuget.org/packages/Voron.AdaptiveLayoutPanel/)
```
Install-Package Voron.AdaptiveLayoutPanel
```

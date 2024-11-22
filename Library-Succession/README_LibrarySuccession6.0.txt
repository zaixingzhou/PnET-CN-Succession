Title:			README_LibrarySuccession4.0
Project:		LANDIS-II Landscape Change Model
Project Component:	Library-Succession
Component Deposition:	https://github.com/LANDIS-II-Foundation/Library-Succession
Author:			LANDIS-II Foundation
Origin Date:		10 May 2017
Final Date:		15 June 2017


Welcome to the source code repository for Library-Succession, a LANDIS-II supplemental library.




##########################
The Science and the Model
##########################

The science powering the LANDIS-II model ultimately resides in .cs files, written in 
the C# programming language. The collection of .cs files associated with the LANDIS-II 
Core Model or with any LANDIS-II extension is the so-called source code. Using the 
source code and the .NET Framework, the actual libraries (.dll files) and executables 
(.exe files) of the LANDIS-II model are constructed. The LANDIS-II model then uses 
various sets of libraries and executables to produce process-based output.

The .NET Framework provides the runtime environment needed for executing C# source code.
Executing C# source code means that the source code is compiled to produce an assembly, either 
a library (.dll file) or an executable (.exe file). The C# code in .cs files cannot be 
independently executed; the use of the .NET Framework is required because the C# programming 
language is so-called 'managed code'.

Integrated development environments (IDEs) are used to assist in compiling .cs files into 
assemblies. Visual Studio and MonoDevelop are two useful IDEs for the C# programming language.
To help with tracking the set of .cs files that are to be compiled, Visual Studio 
creates 'container' files called 'projects' and 'solutions'. A 'project' is the collection of 
source code files that the C# compiler will combine to produce a single output (an assembly). 
A Visual Studio project file is designated with a .csproj extension. A 'solution' is a set of 
one or more .csproj files.  A Visual Studio solution file is designated with a .sln extension.


The process of building 'the science' into 'the model' is done via a LANDIS-II extension.
The process looks like this:

==> a set of .cs files is created or modified that translates process-based science into 
    algorithms, and from the algorithms, into C# source code (script)

  ==> a .csproj file is created that links the various .cs files together within an IDE

    ==> the IDE takes the set of .cs files plus the .NET Framework and 'builds' the requisite 
	assemblies: libraries (.dll files) and executables (.exe files). LANDIS-II extensions
	consist ONLY of libraries.

        ==> the newly-built assemblies constitute 'the extension' and are packaged into 
	    a Windows-based installer (a Wizard)

          ==> LANDIS-II users run the Wizard which installs the extension (a set of assemblies)
	      into the following directory: "C:\Program Files\LANDIS-II\v6\bin\extensions\" 


##############################################
Preliminary notes for building a new or 
modified supplemental library from source code
#################################################

NB. "Landis.Library.Succession.dll" and "Landis.Library.Succession.pdb" can be re-built
by loading the "library-succession.csproj" file into Visual Studio (or MonoDevelop)
and re-building. There is no installer for "Landis.Library.Succession.dll" because it
is not a LANDIS-II extension but rather a supplemental library.

NB. It is recommended that you use Git for version control and change tracking.
This means cloning the repository to your local machine.
Help with Git functionality can be found in the ProGit Manual (freely available)
as a .pdf (https://git-scm.com/book/). A very straighforward Windows/Git interface 
is "git BASH" (available at https://git-for-windows.github.io/)

NB. Should you want the LANDiS-II Foundation to consider your changes for inclusion in
the LANDIS-II Foundation's main GitHub repository (https://github.com/LANDIS-II-Foundation/)
you will need to submit a Pull request.

NB. Visual Studio (VS) may mark references to some libraries as "unavailable" until the 
solution is actually (re)built. During the build process, VS will automatically retrieve any 
requisite libraries (assmeblies) from the Support-Library-Dlls repository, located at
https://github.com/LANDIS-II-Foundation/Support-Library-Dlls. Retrieval of requisite libraries 
is done by running the script, "install-libs.cmd" as a pre-build event. 

NB. Libraries such as "System" and "System.Core" are assemblies that should be available on 
your machine as part of the .Net Framework. For example, examining "System" and "System.Core" 
in References (Solution Explorer ==> References) yields the following output in an Object 
Browser window in VS,

Assembly System
    C:\Windows\Microsoft.NET\Framework\v2.0.50727\System.dll

Assembly System.Core
    C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.5\System.Core.dll






This repository contains a copy backup of the source texts of the "Form Transformation Tool"provided by Microsoft on 7/5/2011. The source code can still be downloaded today as described in [this](https://community.dynamics.com/nav/b/navericwauters/archive/2011/08/17/source-code-of-form-transformation-tool-published) Blogtext. These are only the unzipped files.

Here's the original readme text:

1) Unzip the package and accept the license agreement.  Note that unzipping will create 3 subdirectories, an ObjectTransformation directory that contains the logic for the transformation core, a FormTransformation directory that provides the shell to run the logic and a run directory which contains the non localized transformation input files required to run the tool.  

2) Open ObjectTransformation.csproj from the ObjectTransformation directory in Visual Studio.

3) Compile the project, this should succeed with 0 errors and 0 warnings and result in the generation of debug binaries in the un subdirectory.

4) Close the project.

5) Open FormTransformation.csproj from the ObjectTransformation directory in Visual Studio.

6) Compile the project, this should succeed with 0 errors and 0 warnings and result in the generation of debug binaries in the run subdirectory.  You can close the project.

7) After adding the approrpiate Forms.xml input file to the run/debug directory, you can run the tool by executing the file Microsoft.Dynamics.Nav.Tools.FormTransformation.exe

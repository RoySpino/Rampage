# Ramage
C# version of my RPG4 compiler

This project was developed so that RPG development could be handled on other platforms other than IBMi. 

# Things to know
This project is still in the interpreter phase.

Like RPG4 this compiler supports fixed and free-format code. But the primary difference between this compiler and IBM RPG is that the spec column begins at column 1 not 6

To make the development simpler there is no distinction between procedures that return nothing and subroutines. The only difference will be the positioning of theses sections of code.

Current specs that are implanted are D, C and P specs

## TODO
Because here is no in-built database, this compiler will rely on an external database. Database assignment and connection will be handled within the H spec.

Add data-structures currently only standalone variable are supported.

F spec implementation:

I spec implementation

O specs implementation: this will output a text file.


This Project is biased on the tutorial series from Immo Landwerth
https://www.youtube.com/watch?v=wgHIkdUQbp0&list=PLRAdsfhKI4OWNOSfS7EUu5GRAVmze1t2y

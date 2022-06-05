# Rampage
You cant spell rampage without RPG.

This project was developed so that RPG development could be handled on a platform other than IBMi. 

# Things to know
This project is still in the interpreter phase.

Free-format and fix-format code is handled by a function "checkFree()" in Lexer.cs. This function returns True/False by checking if the first symbol in the source file is "**free". 

Like RPG4 this compiler supports fixed and free-format code. Ideally code should be written in column 1, however the compiler can handle traditional RPG code that begins at column 6. 

To make the development simpler there is no distinction between procedures that return nothing and subroutines. The only difference will be the positioning of theses sections of code. Subroutines are C specs so they should appear before P specs.

Current specs that are implanted are D, C and P specs. H spec is implemented as well, but it currently only handles the NOMAIN key word. 

## TODO
Because there is no in-built database, this compiler will rely on an external database. Database assignment and connection will be handled within the H spec.

Add data-structures currently only standalone variable are supported.

F spec implementation:

I spec implementation

O specs implementation: this will output a text file.


This Project is biased on the tutorial series from Immo Landwerth
https://www.youtube.com/watch?v=wgHIkdUQbp0&list=PLRAdsfhKI4OWNOSfS7EUu5GRAVmze1t2y

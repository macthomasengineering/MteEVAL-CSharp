# MteEVAL - Simple C# Expression Compiler and Eval Library

MteEVAL is a library for compiling and evaluating expressions at runtime. Expressions are converted to bytecode and then executed on demand with a simple virtual machine.

There are five editions of the library:

* .NET (C#)
* Android (B4A)
* iOS (B4i)
* Java (B4J)
* JavaS2(B4A/B4J)

   *This repository is for our .NET edition of the library in native C#.*

See [Anywhere Software](https://www.b4x.com) to learn more about B4A, B4i, and B4J cross-platform development tools.

## Application

Creating expressions at runtime is a powerful tool allowing calculations and program flow to be modified after installation, which otherwise would require a physical update or a custom build of an application. For example, any application designed to manage a sales compensation plan could benefit from runtime expressions, where the end-user may want to customize the plan's formulas by team members, product mixes and sales goals.

## Codeblocks

MteEVAL implements the expression compiler through the class Codeblock. MteEVAL's codeblock adopts the syntax from the venerable 1990's xBase compiler [Clipper 5](https://en.wikipedia.org/wiki/Clipper_(programming_language)) where the construct began. Codeblocks start with an open brace, followed by an optional parameter list between pipes, then the expression, and end with a closing brace.

```clipper
{|<parameters>|<expression>}
```
## Operator support

The library supports C/Java style operators along side a growing list of C# and B4X compatible methods.

* Math operators: +-*/%
* Math constants: cpi, ce 
* Relational: > < >= <= != ==
* Logical: || && !
* Bitwise: << >> & ^ |
* Assignment: =
* Functions: abs(), ceil(), floor(), iif(), if(), min(), max(), sqrt(), power(), round()
* Trig Functions: acos(), acosd(), asin(), asind(), atan(), atand(), cos(), cosd(), sin(), sind(), tan(), tand()

## Examples

You only need to compile a Codeblock once.  Once compiled you can evaluate it as many times as needed, all while supplying different arguments. All arguments and the return values are type *double.*

Example 1: Codeblock without parameters

```cs
Codeblock cb = new Codeblock();
cb.Compile("{||5 + 3}");
double result = cb.Eval();           // result=8
```

Example 2: Codeblock with parameters

```cs
Codeblock cb = new Codeblock();
cb.Compile( "{|length,width|length*width}" );
double area = cb.Eval(3, 17);        // area=51
```

Example 3: Codeblock compile, eval and repeat

```cs
Codeblock cb = new Codeblock();
cb.Compile("{|sales,r1,r2| r1*sales + iif( sales > 100000, (sales-100000)*r2, 0 ) }");
double commission1 = cb.Eval( 152000, .08, .05 );    // commission1=14760
double commission2 = cb.Eval( 186100, .08, .07 );    // commission2=20915
double commission3 = cb.Eval( 320000, .08, .05 );    // commission3=36600
```

Example 4: Codeblock with optimization disabled

```cs
Codeblock cb = new Codeblock();
cb.OptimizerEnabled = false;                       
cb.Compile("{|sales,r1,r2| r1*sales + iif( sales > 100000, (sales-100000)*r2, 0 ) }");
List<string> codelist = cb.Decompile();
foreach (var item in codelist)  Console.WriteLine(item);
```
```
-- Header --
Parameters=3
-- Code --
1:     loadv   ax, varmem[1]
3:     push    ax
4:     loadv   ax, varmem[0]
6:     mul     stack[sp] * ax
6:     pop     
7:     push    ax
8:     loadv   ax, varmem[0]
10:    push    ax
11:    loadc   ax, 100000
13:    gt      stack[sp] > ax
13:    pop     
14:    jumpf   28
16:    loadv   ax, varmem[0]
18:    push    ax
19:    loadc   ax, 100000
21:    sub     stack[sp] - ax
21:    pop     
22:    push    ax
23:    loadv   ax, varmem[2]
25:    mul     stack[sp] * ax
25:    pop     
26:    jump    30
28:    loadc   ax, 0
30:    add     stack[sp] + ax
30:    pop     
31:    end     
```
Example 5: Codeblock with peephole optimization (enabled by default)

```cs
Codeblock cb = new Codeblock();
cb.Compile("{|sales,r1,r2| r1*sales + iif( sales > 100000, (sales-100000)*r2, 0 ) }");
List<string> codelist = cb.Decompile();
foreach (var item in codelist)  Console.WriteLine(item);
```
```
-- Header --
Parameters=3
-- Code --
1:     pushv   varmem[1]
3:     loadv   ax, varmem[0]
5:     mul     stack[sp] * ax
5:     pop     
6:     push    ax
7:     pushv   varmem[0]
9:     loadc   ax, 100000
11:    gt      stack[sp] > ax
11:    pop     
12:    jumpf   25
14:    pushv   varmem[0]
16:    loadc   ax, 100000
18:    sub     stack[sp] - ax
18:    pop     
19:    push    ax
20:    loadv   ax, varmem[2]
22:    mul     stack[sp] * ax
22:    pop     
23:    jump    27
25:    loadc   ax, 0
27:    add     stack[sp] + ax
27:    pop     
28:    end     
```
   *The optimizer reduced the code size from 31 to 28.*
## Linking to your project

* ToDo

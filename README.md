#MteEVAL - Simple C# Expression Compiler and Eval Library

MteEVAL is a library for compiling and evaluating expressions at runtime. Expressions are converted to bytecode and then executed on demand with a simple virtual machine.

There are five editions of the library: C#, Android (B4A), iOS (B4i), Java (B4J), JavaS2 (B4A/B4J).

   *This repository is for our stage 2 performance edition of the library in native C#.*

See [Anywhere Software](https://www.idevaffiliate.com/33168/16-0-3-1.html) to learn more about B4A, B4i, and B4J cross-platform development tools.

##Application

Creating expressions at runtime is a powerful tool allowing calculations and program flow to be modified after installation, which otherwise would require a physical update or a custom build of an application. For example, any application designed to manage a sales compensation plan could benefit from runtime expressions, where the end-user may want to customize the plan's formulas by team members, product mixes and sales goals.

##Codeblocks

MteEVAL implements a single class named Codeblock. MteEVAL's codeblock adopts the syntax from the venerable 1990's xBase compiler [Clipper 5](https://en.wikipedia.org/wiki/Clipper_(programming_language)) where the construct began. Codeblocks start with an open brace, followed by an optional parameter list between pipes, then the expression, and end with a closing brace.

```clipper
{|<parameters>|<expression>}
```

##Examples

You only need to compile a Codeblock once.  Once compiled you can evaluate it as many times as needed, all while supplying different arguments. 

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
##Operator support

The library supports C/Java style operators along side a growing list of internal functions.

* Math operators: +-*/%
* Relational: > < >= <= != ==
* Logical: || && !
* Bitwise: << >> & ^ |
* Assignment: =
* Functions: abs(), ceil(), floor(), iif(), if(), min(), max(), sqrt(), power(), round()
* Trig Functions: acos(), acosd(), asin(), asind(), atan(), atand(), cos(), cosd(), sin(), sind(), tan(), tand()

##Linking to your project

* ToDo
# CircuitDrawing
This is a small tool that allows you to create vector graphics for electrical circuit diagrams in an easy-to-use way. I have created this program with visual basic .NET during my time at university to create nice-looking circuit diagrams for various papers and scientific presentations. Please see the small Demo here, it contains two simple demo schematics created using this tool and displayed in a LaTeX pdf: https://github.com/SimonGit0/circuit-drawing/blob/master/Demo/Demo.pdf

## Getting Started – Install
The program is designed to run out of the box without installation. All needed binaries to run under windows are in the folder Install_Dir. Just copy this folder to any destination you like and start the main program "CircuitDrawing.exe"

## First steps 
Circuit Drawing is designed to easily create simple diagrams. Just explore the different possibilities. After starting, an empty schematic sheet will pop-up, where you can start placing components. To do so, there is already a list of components on the right side. Just select one (e.g. a resistor) and place it by pressing the shortcut key "I" (or via Edit -> Add Instance). Also observe the hints in the status bar on the lower left of the screen with further hints and helpful shortcut keys. If you are done placing components you always exit the current action/tool by pressing the key ESC. Nearly all the components that are already build-in come with a variety of options and configurations (e.g. different style of transistors, resistors). Just select one placed component in your schematic and see all the options in the Window "Settings" on the left side of your screen. As there are many possible components, they are grouped in different categories. The default category is “Basic” with the most basic components like resistors, capacitors, … You can explore the other categories by changing the category at the top of the list. The categories and components are named by their German name currently – but you see their picture anyways.  

The routing can be done by Edit -> Add Wire (or simply by using the shortcut key "W"). You can either click twice (once for starting point and once for ending point) or use the shortcut key "S" (= snap). Using "S" instead of a click will attach the wire to the nearest snap point of any component (visualized by a red circle). Again, if you are done routing wires, use ESC to exit this action. 

From there on you have many more possibilities to edit and modify your schematic. Select one or more instances and press "M" to move them, modify their properties, add labels, or many more… 

## Add labels
Labels are very important for any schematic diagram to make them easily readable. Thus, lots of different labels can be created.

### Automatic labels of each component
Each component has a default label attached directly next to it after placing it. Doing so, all the labels are nicely attached in a consistent manner in your whole schematic. The label itself can be edited in the properties of the component ("Text"). Also you can cycle through multiple default positions for this label by pressing the space bar while the element is selected. If none of the defaut positions fits your needs, you can also fine-tune the position by the "Distance" setting the properties of the component. It will just add a vector to the current position.

### Additional labels
Many additional labels can be attatched to your circuit diagram. You can place them via the menu Edit -> Add label -> select your needed label. There are labels for voltages or currents, for impedances, for buses, or just generic labels. 
All of these labels are then attached to one of your wires and they will automatically move with the wire if the wire is moved. 

## Save, Open and Export
Of course, you can save the schematic and reopen it later to continue. The schematic is saved in a custom binary format with all the information needed to open it later. Just use the "Save" and "Open" buttons.
To export you finished graphics you can use the different options in the menu File -> Export ->. Ideally, the schematic is exported as vector graphics (PDF, EMF, ...). But as a backup you can also export it to a simple bitmap (png, jpg, ...) and specify any image size you like.

### Export as EMF
The simplest way is to export your schematic to an .emf. This way you can open it with most image tools and it can also be included in Office programs as a nice-looking vector graphics.

### Export for LaTeX
If you use LaTeX to create documents you can export directly as a PDF + LaTeX (TEX + PDF). This will create two output files. One .tex file that contains all the text and annotations and one underlaying pdf with the basic drawings. To include it in your LaTeX document just include the .tex file. It will automatically include the pdf and place it at the right position. See below for an example LaTeX code to include the created figure:

\begin{figure}

\centering

\input{MyExportedCircuit.tex}

\caption{Exported schematic drawn with Circuit Drawing}

\end{figure}


If you need more advanced options you can set a few optional parameters in your LaTeX source code to modify the schematic import. You can set \def\scaleSchematic{0.8} before importing your schematic to scale the picture (e.g. to make it a little smaller to squeeze it into the page). The parameter 0.8 would mean the schematic is scaled to 80% of its original size. To scale the font size use any LaTeX font changing command before importing the schematic.

Sometimes you also place your pictures in a different subfolder if you have a large LaTeX document. Doing so, the import of the pdf inside the .tex document does not work and you will see an error. To fix this, you can set the parameter \def\pathSchematic{subfolder/} to your desired subfolder. This way you can tell LaTeX to look for the pdf in this folder. If you place all pictures in the same subfolder you can also add this command once at the beginning of your LaTeX document. Below is an LaTeX import example using more advanced import settings:
\def\pathSchematic{subfolder/} % all my figures are stored in the subfolder "subfolder/"

\begin{figure}

\centering

\def\scaleSchematic{0.9} % I want this figure scaled to 90% its original size

\footnotesize % I also want smaller text size (footnotesize) in my schematic drawing

\input{subfolder/MyExportedCircuit.tex}

\caption{Exported schematic drawn with Circuit Drawing}

\end{figure}

## Custom Symbols
Many symbols are already available for various circuit diagrams. Also most of them are configurable to create many different variants (e.g. different types of diodes, different drawing styles of resistors, transistors or digital gates, …). But of course, at some point you need a special symbol that is not present yet. The program loads all available symbols from a database stored currently in the folder ET in the install path. There you find the definition for all the existing symbols. To create a new symbol just copy one of the old ones and modify it to your needs. I do not have the time now to document the syntax for creating new symbols, but just explore it a bit ;)
Note: new symbols are read-in currently only at start of the program. So when you modify, add or change any symbol you need to close and re-open the tool to have it available

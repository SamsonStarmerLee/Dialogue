# Dialogue

An in-progress pattern-matching approach to dialogue events, based on the theory behind Valve's 2012 GDC talk: 
[AI-driven Dynamic Dialog through Fuzzy Pattern Matching](https://youtu.be/tAbBID3N64A)

Dialogue is stored as a series of rules, ordered from the specific to the general. Each rule can test any state about 
triggering events, relevant characters or the state of the world. 

When a rule is successful, the related dialogue is printed out, and the rule can then write back some state,
for keeping track of relevent information. In this way, writers can create arbitrary data for tracking thing like 
running jokes, sequential lines or properties about the world (without needing a programmer to expose them).

These dialogue rules, broken up as state-checking Criteria and state-writing Rememberers, are written out in 
Excell/google sheets/etc and exported as a CSV, for bringing into Unity. 

## State

The information accessible to a rule and its criteria/rememberers is broken up into 4 categories. Note that 'State'
cannot be written to, whereas 'Memory' is read-write.

### Event State

The triggering event (say, looking at an object) can bundle whatever additional state it wants as part of the call.
The project currently includes a "SeeObject" event, which includes information about the object looked at.

### Character State

The character responsible for the triggering event provides information about its own state, such as position, orientation,
name, etc.

### Character Memory

Any instance/object can have arbitrary state assigned to it. GameObjects that don't implement `IMemoryRetainer` have a
`MemoryContainer` component attached dynamically. This state can be whatever the writer wishes, and is useful to allow
a character to remember what it has said, seen, and done.

### World Memory
The global equivalent of character memory. This enables many actors to coordinate; for example, multiple characters
searching for objects could collectively know how many had been found.

## Formatting

When writing a criteria/remember, the checks are appended with a character associated with each of the above:
- **'e'**: Event State
- **'c'**: Character State
- **'m'**: Character Memory
- **'w'**: World Memeory
- **'t'**: Shortcut for accessing the memory of a 'target' character, if there is one.

There are some reserved strings, such as 'true' and 'false'. These are explained in `RuleInterpreter.cs`.



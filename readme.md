# Specification
## Notes
### Theory
JKL; defines note (release all for rest)
if the software detects a change in note, it will wait an adjustable time before then playing the currently held note to allow time for changing finger positions
Specific note controls will be data-driven.

## Octaves
SDF controls octave
hold D to temporarily shift to a lower octave
hold F to temporarily shift to a higher octave
press S while holding D/F to make that shift permanent (you can then relase D/F)

# Future design plans
consider adding a system which connects the ordered notes in a circle automatically by shifting the ocatve up/down if you transition from a note that is at the top/bottom of the octave into one that is at the opposite end and their distance (ignoring octave) is <= 2 semitones.

consider uses for 'A' key

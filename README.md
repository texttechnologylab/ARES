 [![License: AGPL v3](https://img.shields.io/badge/License-AGPL_v3-blue.svg)](https://www.gnu.org/licenses/agpl-3.0)


# ARES: Annotation von Relationen und Eigenschaften zur Szenengenerierung

## Abstract
Im Fachbereich der Computerlinguistik ist die automatische Generierung von Szenen aus, in natürlicher Sprache verfassten, Text seit bereits vielen Jahrzehnten ein wichtiger Bestandteil der Forschung, welche in der "Kunst", "Lehre" und "Robotik" Verwendung finden. Mit Hilfe von neuen Technologien im Bereich der Künstlichen Intelligenzen (KI), werden neue Entwicklungen möglich, welche diese Generierungen vereinfachen, allerdings auch undurchsichtige interne vom Modell getroffene Entscheidungen fördern. 

Ziel der vorgeschlagenen Lösung "ARES: Annotation von Relationen und Eigenschaften zur Szenengenerierung" ist es, ein modulares System zu entwerfen, wobei einzelne Prozesse für den Benutzer verständlich bleiben. Außerdem sollen Möglichkeiten geboten werden, neue Entitäten und Relationen, welche über die Textanalyse bereitgestellt werden, auch in die Szenengenerierung im dreidimensionalen Raum einzupflegen, ohne dass hierfür Code zwingend notwendig wird. Der Fokus liegt auf der syntaktisch korrekten Darstellung der Elemente im Raum. Dagegen lässt sich die semantische Korrektheit durch weitere manuelle Anpassungen, welche für spätere Generierungen gespeichert werden erhöhen. 

Letztlich soll die Menge der zur Darstellung benötigten Annotationen möglichst gering bleiben und neue szenenbezogene Annotationen durch die implementierten Annotationstools hinzugefügt werden.

## How to Setup the Environment

### Use the requirements file for missing Pyhton modules or enter
```python
pip install flask
pip install spacy
pip install numerizer
python -m spacy download en_core_web_sm
```
### Enter following lines in the terminal inside the NLP Folder
```python
set FLASK_ENV=development
set FLASK_APP=main.py
py -m flask run
```
### Run the 3DEnvironment Project in the Unity Editor 
First Scene is "Main Menu"

# Usage & Cite
Follow the instructions embedded into the scenes and read the related Paper. If you want to use the project, please quote this as follows:

Patrick Masny. ARES: Annotation von Relationen und Eigenschaften zur Szenengenerierung , bachelor’s thesis, Frankfurt, Germany, 2022. [[Link]](https://publikationen.ub.uni-frankfurt.de/frontdoor/index/index/docId/70595) [[PDF]](https://publikationen.ub.uni-frankfurt.de/files/70595/ARES___Bachelorarbeit_Patrick_Masny.pdf)

# BibTeX
```
@thesis{Masny:2022,
  author    = {Patrick Masny},
  title     = {ARES: Annotation von Relationen und Eigenschaften zur Szenengenerierung},
   institution = {Institute of Computer Science and Mathematics, Text Technology Lab},
  address = {Frankfurt, Germany},
  type = {bachelor's thesis},
  pages     = {48},
  year        = {2022},
  pdf       = {https://publikationen.ub.uni-frankfurt.de/files/70595/ARES___Bachelorarbeit_Patrick_Masny.pdf},
  url       = {https://publikationen.ub.uni-frankfurt.de/frontdoor/index/index/docId/70595}
}

```

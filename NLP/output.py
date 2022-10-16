import spacy
import numerizer

from helper import remove_duplicates_in_list, starts_with_item_of_list, get_unique_relations
from dependency import Dependency
from pos import POS
from token_rep import Token
import random as rd

KNOWN_UNWANTED_NOUNS = ['leave', 'right', 'side', 'room', 'wall', 'front']


class SpacyOutputHandler:

    def __init__(self, input_text):
        self.nlp = spacy.load("en_core_web_sm")
        self.doc = self.nlp(input_text)
        self.text = input_text
        self.dependency_lst = []
        self.pos_lst = []
        self.token_lst = []

    def spacy_dependency_tagger(self):
        for token in self.doc:
            dependency = Dependency(begin=token.head.i, end=token.i, value=token.dep_)
            self.dependency_lst.append(dependency)

    def spacy_pos_tagger(self):
        for token_nr in range(len(self.doc)):
            pos = POS(index=token_nr, value=self.doc[token_nr].tag_)
            self.pos_lst.append(pos)

    def spacy_token_tagger(self):
        for token_nr in range(len(self.doc)):
            pos = Token(index=token_nr, value=self.doc[token_nr].text, lemma=self.doc[token_nr].lemma_)
            self.token_lst.append(pos)

    def fix_lemma(self):
        for token_nr in range(len(self.token_lst)):
            entity_name = self.token_lst[token_nr].lemma
            if entity_name == "book":
                if self.rel_in_rel_list(token_nr, "on", "shelf") or self.rel_in_rel_list(token_nr, "with", "shelf"):
                    self.token_lst[token_nr].lemma = "standbooks"
                if self.rel_in_rel_list(token_nr, "on", "cabinet") or self.rel_in_rel_list(token_nr, "with", "cabinet"):
                    self.token_lst[token_nr].lemma = "stackbooks"

            if self.token_lst[token_nr].lemma == 'sofa':
                self.token_lst[token_nr].lemma = 'couch'
            if self.token_lst[token_nr].lemma == 'pc':
                self.token_lst[token_nr].lemma = 'computer'
            if self.token_lst[token_nr].lemma == 'mouse':
                self.token_lst[token_nr].lemma = 'computermouse'
            if self.token_lst[token_nr].lemma == 'shelf':
                self.token_lst[token_nr].lemma = 'bookcase'
            if self.token_lst[token_nr].lemma == 'headphones':
                self.token_lst[token_nr].lemma = 'headphone'
            if self.token_lst[token_nr].lemma == 'socket':
                self.token_lst[token_nr].lemma = 'powerstrip'
            if self.token_lst[token_nr].lemma == 'clock':
                self.token_lst[token_nr].lemma = 'tableclock'
            if self.token_lst[token_nr].lemma == 'frame':
                self.token_lst[token_nr].lemma = 'framework'
            if self.token_lst[token_nr].lemma == 'lamp':
                self.token_lst[token_nr].lemma = 'desklamp'
            if self.token_lst[token_nr].lemma == 'cabinet':
                self.token_lst[token_nr].lemma = 'filecabinet'

    def create_output(self):
        result = dict()

        output = []
        scene = dict()

        tokens = [token.strValue for token in self.token_lst]
        scene['tokens'] = tokens

        found_entities = self.extract_entities()

        entities = []
        for entity_nr in found_entities:
            entity = dict()
            entity['index'] = self.token_lst[entity_nr].index
            entity['lemma'] = self.token_lst[entity_nr].lemma
            entity['amount'] = self.assign_counts(entity_nr)
            entities.append(entity)
        scene['entities'] = entities

        relations = []
        for token_nr in range(len(self.token_lst)):
            for rel, end in self.assign_relationships(token_nr):
                relation = dict()
                relation['head'] = token_nr
                relation['tail'] = end
                relation['type'] = rel
                relations.append(relation)
        scene['relations'] = get_unique_relations('head', 'tail', 'type', relations)

        attributes = []
        for token_nr in found_entities:
            for attr in self.assign_entity_attributes(token_nr):
                attribute = dict()
                attribute['index'] = token_nr
                attribute['type'] = attr
                attributes.append(attribute)
        scene['attributes'] = attributes

        output.append(scene)
        result['result'] = output
        return result

    def extract_entities(self):
        """
        Extracts and returns all entities (as token number) of the cas string
        """
        pos_lst = self.pos_lst
        token_lst = self.token_lst
        dep_lst = self.dependency_lst

        entities = []
        for pos in pos_lst:
            if pos.tagValue.startswith('NN'):
                entities.append(pos.index)

        # remove nouns that are compounds
        for dep in dep_lst:
            if dep.depValue == 'compound' or dep.depValue == 'amod':
                try:
                    entities.remove(dep.end)
                except ValueError as e:
                    print(e)

        # remove left, right
        for token in token_lst:
            if token.lemma in KNOWN_UNWANTED_NOUNS:
                try:
                    entities.remove(token.index)
                except ValueError as e:
                    print(e)

        return entities

    def assign_counts(self, token_nr):
        """
        returns the amount of entity instances for the according token_nr
        """
        count = 1
        det_text = ''
        token_lst = self.token_lst
        dep_lst = self.dependency_lst
        doc = self.doc
        try:
            det_token_nr = next(
                x.end for x in dep_lst if x.depValue == 'nummod' and x.begin == token_nr)
            det_text = token_lst[det_token_nr].strValue
        except:
            count = 1
        for key in doc._.numerize().keys():
            count = doc._.numerize()[key]

        attributes = self.assign_entity_attributes(token_nr)
        determiners = self.get_determiners(token_nr)
        possible_instance_count_strings = attributes + determiners

        entity_name = token_lst[token_nr].lemma
        if entity_name == "monitor":
            count = 1
        if entity_name == "keyboard":
            count = 1
        else:
            if "some" in possible_instance_count_strings:
                count = rd.randint(2, 4)
            if "many" in possible_instance_count_strings:
                count = rd.randint(3, 6)
            if "monitor" in possible_instance_count_strings:
                count = 1
            if "keyboard" in possible_instance_count_strings:
                count = 1

        if entity_name == "book":
            if self.rel_in_rel_list(token_nr, "on", "shelf") or self.rel_in_rel_list(token_nr, "with", "shelf"):
                count = rd.randint(2, 4)
            if self.rel_in_rel_list(token_nr, "on", "cabinet") or self.rel_in_rel_list(token_nr, "with", "cabinet"):
                count = 1

        # for single form
        if "the" in determiners:
            count = 1

        return count

    def rel_in_rel_list(self, token_nr, rel_search, obj_search):
        """
        returns if the relation (with the type and object) already exists in the relation list
        """
        tokens = self.token_lst
        rels = self.assign_relationships(token_nr)
        for rel_type, end_token_nr in rels:
            if rel_type == rel_search and tokens[end_token_nr].lemma == obj_search:
                return True
        return False

    def get_determiners(self, token_nr):
        """
        returns a list with the determiner for the given token_nr
        """
        dep_lst = self.dependency_lst
        tokens = self.token_lst
        try:
            det = next(x for x in dep_lst if x.depValue == 'det' and x.begin == token_nr)
            return [tokens[det.end].strValue]
        except:
            return []

    def assign_relationships(self, token_nr):
        """
        returns the relationships of an entity for the according token_nr
        """
        dep_lst = self.dependency_lst
        token_lst = self.token_lst
        relationships_token_nr = []
        tokens = token_lst
        # around
        # ->can include unnecessary relations
        for det1 in [x for x in dep_lst if
                     x.depValue.startswith('prep') and x.begin == token_nr]:
            for det2 in [x for x in dep_lst if
                         x.depValue.startswith('pobj') and x.begin == det1.end]:
                try:
                    relationships_token_nr.append([[det1.end], det2.end])
                except:
                    pass

        # next to
        for det1 in [x for x in dep_lst if
                     x.depValue.startswith('advmod') and x.begin == token_nr]:
            for det2 in [x for x in dep_lst if
                         x.depValue.startswith('prep') and x.begin == det1.end]:
                for det3 in [x for x in dep_lst if
                             x.depValue.startswith('pobj') and x.begin == det2.end]:
                    try:
                        relationships_token_nr.append([[det2.begin, det2.end], det3.end])
                    except:
                        pass

        # on top of, to left of
        for det1 in [x for x in dep_lst if
                     x.depValue.startswith('prep') and x.begin == token_nr]:
            for det2 in [x for x in dep_lst if
                         x.depValue.startswith('pobj') and x.begin == det1.end]:
                for det3 in [x for x in dep_lst if
                             x.depValue.startswith('prep') and x.begin == det2.end]:
                    for det4 in [x for x in dep_lst if
                                 x.depValue.startswith('pobj') and x.begin == det3.end]:
                        try:
                            relationships_token_nr.append([[det2.begin, det3.begin, det4.begin], det4.end])
                        except:
                            pass

        # already in the other relations implemented
        # # under
        # for det1 in [x for x in dep_lst if x.depValue.startswith('attr') and x.end == token_nr]:
        #     for det2 in [x for x in dep_lst if
        #                  x.depValue.startswith('prep') and x.begin == det1.begin]:
        #         for det3 in [x for x in dep_lst if
        #                      x.depValue.startswith('pobj') and x.begin == det2.end]:
        #             try:
        #                 relationships_token_nr.append([[det2.end], det3.end])
        #             except:
        #                 pass

        # # (on) left of
        # for det1 in [x for x in dep_lst if x.depValue.startswith('attr') and x.end == token_nr]:
        #     for det2 in [x for x in dep_lst if
        #                  x.depValue.startswith('prep') and x.begin == det1.begin]:
        #         for det3 in [x for x in dep_lst if
        #                      x.depValue.startswith('pobj') and x.begin == det2.end]:
        #             for det4 in [x for x in dep_lst if
        #                          x.depValue.startswith('prep') and x.begin == det3.end]:
        #                 for det5 in [x for x in dep_lst if
        #                              x.depValue.startswith('pobj') and x.begin == det4.end]:
        #                     try:
        #                         relationships_token_nr.append([[det3.end, det4.end], det4.end])
        #                     except:
        #                         pass

        # surrounded by
        for det1 in [x for x in dep_lst if
                     x.depValue.startswith('nsubjpass') and x.end == token_nr]:
            for det2 in [x for x in dep_lst if
                         x.depValue.startswith('agent') and x.begin == det1.begin]:
                for det3 in [x for x in dep_lst if
                             x.depValue.startswith('pobj') and x.begin == det2.end]:
                    try:
                        relationships_token_nr.append([[det2.begin, det2.end], det3.end])
                    except:
                        pass

        relationships_token_nr = remove_duplicates_in_list(relationships_token_nr)
        relationships = []

        entity_nrs = self.extract_entities()

        for mid, end in relationships_token_nr:
            if (token_nr in entity_nrs and end in entity_nrs):
                relation_string = ''
                for mid_token in mid:
                    relation_string += tokens[mid_token].strValue

                relationships.append([relation_string, end])

        # simplifying post processing
        for rel_idx in range(len(relationships)):
            relation_string, end = relationships[rel_idx]
            if 'on' in relation_string and 'front' not in relation_string and 'along' not in relation_string:
                if 'right' in relation_string:
                    relationships[rel_idx][0] = 'onright'
                if 'left' in relation_string:
                    relationships[rel_idx][0] = 'onleft'

            anchor_obj_name = tokens[token_nr].lemma
            act_obj_name = tokens[end].lemma
            if 'with' in relation_string:
                if 'book' in act_obj_name:
                    act_obj_name = 'book'

                if anchor_obj_name == 'tv' and act_obj_name == 'speaker' or anchor_obj_name == 'bed' and act_obj_name == 'nightstand':
                    relationships[rel_idx][0] = 'side'

                continue

            if 'next to' in relation_string or 'close to' in relation_string or 'near' in relation_string:
                relationships[rel_idx][0] = 'near'
                continue
            if 'front' in relation_string:
                relationships[rel_idx][0] = 'front'
                continue
            if 'back' in relation_string or 'behind' in relation_string:
                relationships[rel_idx][0] = 'back'
                continue
            if 'left' in relation_string:
                relationships[rel_idx][0] = 'leftside'
                continue
            if 'right' in relation_string:
                relationships[rel_idx][0] = 'rightside'
                continue
            if 'under' in relation_string or 'below' in relation_string:
                relationships[rel_idx][0] = 'under'
                continue
            if 'center' in relation_string:
                relationships[rel_idx][0] = 'oncenter'
                continue
            if 'stack' in relation_string:
                relationships[rel_idx][0] = 'stacked'
                continue
            if 'each' in relation_string and 'side' in relation_string:
                relationships[rel_idx][0] = 'leftside'
                continue
            if 'around' in relation_string:
                relationships[rel_idx][0] = 'pairaround'
                continue
            if 'align' in relation_string:
                relationships[rel_idx][0] = 'pairaligned'
                continue

        return relationships

    def assign_entity_attributes(self, token_nr):
        """
        returns the attributes of an entity for the according token_nr
        """
        attributes_token_nr = set()
        token_lst = self.token_lst
        pos_lst = self.pos_lst
        dep_lst = self.dependency_lst
        # Adjectives from nsubj. Here the connecting verb is assumed to be "to be".
        # This may not always be the case; investigate further.
        # ex. "The chairs are wooden."
        for det in [x for x in dep_lst if x.depValue.startswith('nsubj') and x.end == token_nr]:
            try:
                pos = next(x for x in pos_lst if x.index == det.begin)
                if starts_with_item_of_list(pos.tagValue, ['JJ', 'RB', 'VB']):
                    attributes_token_nr.add(det.begin)
            except:
                pass

        # Adjectives from compounds. ex. "There is a dining table."
        for det in [x for x in dep_lst if
                    x.depValue.startswith('compound') and x.begin == token_nr]:
            try:
                pos = next(x for x in pos_lst if x.index == det.end)
                if starts_with_item_of_list(pos.tagValue, ['NN']):
                    attributes_token_nr.add(det.end)
            except:
                pass

        # Adjectives from amod. ex. "a sleek and white laptop"
        for det in [x for x in dep_lst if
                    x.depValue.startswith('amod') and x.begin == token_nr]:
            attributes_token_nr.add(det.end)

        # The chairs are all aligned.
        # nsubjpass(aligned-5, chairs-2~NNS)
        # advmod(aligned-5, all-4~DT)          <- dep?
        # nsubj(0-VB, 1-NN)
        # advmod(0-VB, 2-DT)
        for det_nsubj in [x for x in dep_lst if
                          x.depValue.startswith('nsubj') and x.end == token_nr]:
            for det_advmod in [x for x in dep_lst if
                               x.depValue.startswith('dep') and x.begin == det_nsubj.begin]:
                try:
                    vb0_pos = next(x for x in pos_lst if x.index == det_advmod.begin)
                    nn1_pos = next(x for x in pos_lst if x.index == det_nsubj.end)
                    dt2_pos = next(x for x in pos_lst if x.index == det_advmod.end)
                    if starts_with_item_of_list(vb0_pos.tagValue, ['VB']) and \
                            starts_with_item_of_list(nn1_pos.tagValue, ['NN']) and \
                            starts_with_item_of_list(dt2_pos.tagValue, ['DT']):
                        attributes_token_nr.add(det_advmod.begin)
                        attributes_token_nr.add(det_advmod.end)
                except:
                    pass

        # Distribute utensils formally on the dining table.
        # advmod(utensils-2, formally-3~RB)       <- dep? and doesn't work because compound utensils is removed
        for det in [x for x in dep_lst if
                    x.depValue.startswith('dep') and x.begin == token_nr]:
            try:
                pos = next(
                    x for x in pos_lst if x.index == det.end)
                if starts_with_item_of_list(pos.tagValue, ['RB']):
                    attributes_token_nr.add(det.end)
            except:
                pass

        # Parser failures should be taken as attributes.
        # Clean the desk.
        # dep(clean-1, desk-3~NN)    <- dobj?VB?
        for det in [x for x in dep_lst if
                    x.depValue.startswith('dobj') and x.end == token_nr]:
            try:
                pos = next(
                    x for x in pos_lst if x.index == det.begin)
                if starts_with_item_of_list(pos.tagValue, ['VB']):
                    attributes_token_nr.add(det.begin)
            except:
                pass

        # A plate, a knife and a fork is placed formally in front of each chair.
        # nsubjpass(placed-10, plate-2~NN)
        # conj:and(plate-2, knife-5~NN)
        # nsubjpass(0-VB, 1-NN)
        # conj:and(1-NN, 2-NN)        <-appos?
        for det_nsubj in [x for x in dep_lst if
                          x.depValue.startswith('nsubjpass') and x.end == token_nr]:
            for det_conj in [x for x in dep_lst if
                             x.depValue.startswith('appos') and x.begin == det_nsubj.end]:
                try:
                    vb0_pos = next(x for x in pos_lst if x.index == det_nsubj.begin)
                    nn1_pos = next(x for x in pos_lst if x.index == det_conj.begin)
                    nn2_pos = next(x for x in pos_lst if x.index == det_conj.end)
                    if starts_with_item_of_list(vb0_pos.tagValue, ['VB']) and \
                            starts_with_item_of_list(nn1_pos.tagValue, ['NN']) and \
                            starts_with_item_of_list(nn2_pos.tagValue, ['NN']):
                        attributes_token_nr.add(det_nsubj.begin)
                        attributes_token_nr.add(det_conj.end)
                except:
                    pass

        # a pile of stacked books           <--- relation doesn't exist
        # nmod:of(pile-14, books-17~NNS)
        # nmod:(pile-14, books-17~NNS)
        for det in [x for x in dep_lst if
                    x.depValue.startswith('nmod') and x.begin == token_nr]:
            try:
                pos = next(
                    x for x in pos_lst if x.index == det.end)
                if starts_with_item_of_list(pos.tagValue, ['NN']):
                    attributes_token_nr.add(det.end)
            except:
                pass

        attributes = []
        for attribute_token_nr in attributes_token_nr:
            attributes.append(token_lst[attribute_token_nr].strValue)
        return attributes



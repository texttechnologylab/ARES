import json
import sys
import os

NLP_PATH = os.path.dirname(os.path.dirname(os.path.dirname(os.path.realpath(__file__)))) + '\\NLP'
print(NLP_PATH)
sys.path.append(NLP_PATH)
from output import SpacyOutputHandler

ANNOTATIONS_PATH = "Annotations.json"

RESULT_NAME = "Auswertung_der_Textanalyse.txt"

def extract_info(text):
    soh = SpacyOutputHandler(text)
    soh.spacy_dependency_tagger()
    soh.spacy_pos_tagger()
    soh.spacy_token_tagger()
    soh.fix_lemma()
    return soh.create_output()

def main():
    comp = Comparator()
    comp.start_compare()
    comp.save_results(RESULT_NAME)

class Comparator:
    def __init__ (self):
        self.entity_true_positive = 0
        self.entity_false_positive = 0
        self.entity_false_negative = 0
        self.relation_true_positive = 0
        self.relation_false_positive = 0
        self.relation_false_negative = 0
        self.total_sentences = 0
        self.total_entities = 0
        self.total_relations = 0
        self.total_tokens = 0
        self.spacy_relation_types = set()
        self.annotaion_relation_types = set()



    def start_compare(self):
        self.reset_results()
        annotations = self.load_json(ANNOTATIONS_PATH)
        for annotation in annotations:
            spacy_analysis = extract_info(' '.join(annotation['tokens']))
            self.save_relation_types(annotation, spacy_analysis)
            self.compare_entity_outputs(annotation, spacy_analysis)
            self.compare_relation_outputs(annotation, spacy_analysis)
            self.total_sentences += 1
            self.total_entities += len(annotation['entities'])
            self.total_relations += len(annotation['relations'])
            self.total_tokens += len(annotation['tokens'])
            print('Processed... ' + str(self.total_sentences) + '/' + str(len(annotations)))

    def save_relation_types(self, annotation, spacy_analysis):
        for relation in annotation['relations']:
            self.annotaion_relation_types.add(relation['type'])
        for spacy_relation in spacy_analysis['result'][0]['relations']:
            self.spacy_relation_types.add(spacy_relation['type'])

    def reset_results(self):
        self.entity_true_positive = 0
        self.entity_false_positive = 0
        self.entity_false_negative = 0
        self.relation_true_positive = 0
        self.relation_false_positive = 0
        self.relation_false_negative = 0
        self.total_sentences = 0
        self.total_entities = 0
        self.total_relations = 0
        self.total_tokens = 0
        self.spacy_relation_types = set()
        self.annotaion_relation_types = set()

    def compare_entity_outputs(self, annotation, spacy_analysis):
            spacy_entities = spacy_analysis['result'][0]['entities']
            annotation_entities = annotation['entities']
            tmp_annotation_entities = annotation['entities'].copy()

            entity_hit = 0
            entity_miss = 0

            for spacy_entity in spacy_entities:
                for ann_ent_nr in range(len(tmp_annotation_entities)):
                    ann_ent = tmp_annotation_entities[ann_ent_nr]
                    if self.is_same_entity(spacy_entity['index'], ann_ent['start'], ann_ent['end']):
                        entity_hit += 1
                        del tmp_annotation_entities[ann_ent_nr]
                        break
                    else:
                        if ann_ent_nr == len(tmp_annotation_entities) - 1:
                            entity_miss += 1

            # print('Entity Recall: ' + str(self.calc_recall(entity_hit, len(tmp_annotation_entities))))
            # print('Entity Precision: ' + str(self.calc_precision(entity_hit, entity_miss)))
            # print('Entity F1: ' + str(self.calc_f1(entity_hit, entity_miss, len(tmp_annotation_entities))))

            self.entity_true_positive += entity_hit
            self.entity_false_positive += entity_miss
            self.entity_false_negative += len(tmp_annotation_entities)

    def compare_relation_outputs(self, annotation, spacy_analysis):
        annotation_entities = annotation['entities']

        spacy_relations = spacy_analysis['result'][0]['relations']
        annotation_relations = annotation['relations']
        tmp_annotation_relations = annotation['relations'].copy()

        relation_hit = 0
        relation_miss = 0

        for spacy_relation in spacy_relations:
            spacy_rel_head_index = spacy_relation['head']
            spacy_rel_tail_index = spacy_relation['tail']
            for ann_rel_nr in range(len(tmp_annotation_relations)):
                ann_rel = tmp_annotation_relations[ann_rel_nr]
                ann_head_ent = annotation_entities[ann_rel['head']]
                ann_tail_ent = annotation_entities[ann_rel['tail']]
                if  self.is_same_entity(spacy_rel_head_index, ann_head_ent['start'], ann_head_ent['end']) and \
                    self.is_same_entity(spacy_rel_tail_index, ann_tail_ent['start'], ann_tail_ent['end']):
                    relation_hit += 1
                    del tmp_annotation_relations[ann_rel_nr]
                    break
                else:
                    if ann_rel_nr == len(tmp_annotation_relations) - 1:
                        relation_miss += 1

        # print('Relation Recall: ' + str(self.calc_recall(relation_hit, len(tmp_annotation_relations))))
        # print('Relation Precision: ' + str(self.calc_precision(relation_hit, relation_miss)))
        # print('Relation F1: ' + str(self.calc_f1(relation_hit, relation_miss, len(tmp_annotation_relations))))

        self.relation_true_positive += relation_hit
        self.relation_false_positive += relation_miss
        self.relation_false_negative += len(tmp_annotation_relations)

    def is_same_entity(self, spacy_index, annotation_start, annotation_end):
        return spacy_index >= annotation_start and spacy_index < annotation_end

    def calc_recall(self, true_positive, false_negative):
        return true_positive / (true_positive + false_negative)

    def calc_precision(self, true_positive, false_positive):
        return true_positive / (true_positive + false_positive)

    def calc_f1(self, true_positive, false_positive, false_negative):
        return 2*true_positive / (2*true_positive + false_positive + false_negative)

    def load_json(self, path):
        with open(path, encoding="utf8") as data:
            return json.load(data)

    def save_results(self, path):
        text = ''
        text += 'Total Sentences: ' + str(self.total_sentences) + '\n'
        text += 'Total Tokens: ' + str(self.total_tokens) + '\n'
        text += 'Total Entities: ' + str(self.total_entities) + '\n'
        text += 'Total Relations: ' + str(self.total_relations) + '\n'
        text += '\n'
        text += 'Entity Recall: ' + str(self.calc_recall(self.entity_true_positive, self.entity_false_negative)) + '\n'
        text += 'Entity Precision: ' + str(self.calc_precision(self.entity_true_positive, self.entity_false_positive)) + '\n'
        text += 'Entity F1: ' + str(self.calc_f1(self.entity_true_positive, self.entity_false_positive, self.entity_false_negative)) + '\n'
        text += 'Relation Recall: ' + str(self.calc_recall(self.relation_true_positive, self.relation_false_negative)) + '\n'
        text += 'Relation Precision: ' + str(self.calc_precision(self.relation_true_positive, self.relation_false_positive)) + '\n'
        text += 'Relation F1: ' + str(self.calc_f1(self.relation_true_positive, self.relation_false_positive, self.relation_false_negative)) + '\n'
        text += '\n'
        text += 'Annotation Diff Relation Type Amount: ' + str(len(self.annotaion_relation_types)) + '\n'
        text += 'Spacy Diff Relation Type Amount: ' + str(len(self.spacy_relation_types)) + '\n'

        f = open(path, 'w', encoding='utf-8')
        f.write(text)
        f.close()


if __name__ == '__main__':
    main()
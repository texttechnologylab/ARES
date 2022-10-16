"""
responsible for converting the exisiting xml files into json
"""

import os
import xml.etree.ElementTree as ET
import spacy
import en_core_web_sm
import re
from subText import subText

NLP = en_core_web_sm.load()

START_FOLDER = 'MergedV0\\'
SAVE_AS =  'Annotations'

RESULT_FOLDER = 'All\\'


cur_index = 0


se_types_dynamic = [['OTHER', 'OTHER']]

rel_types_dynamic = []


def main():
    """
    main function
    """
    jsonAll = '['
    file_dics = getSpecificFileDirectories(START_FOLDER, '.xml')
    file_nr = 1
    for file_directory in file_dics:
        print(file_nr, '/', len(file_dics))
        file_nr += 1
        text, start_ends, spatial_entities, links = getTextAndInfosFromXmlFile(file_directory)
        sentences = splitToSentences(text)
        new_sentences = []
        for sentence in sentences:
            stexts = splitText(sentence, start_ends)
            for sub_text in stexts:
                sub_text.tokens = textToTokens(sub_text.text)
            stexts.sort(key=lambda x: x.start, reverse=False)
            new_sentences.append(stexts)
        json = '['

        polished_new_sentences = []
        for sentence in new_sentences:
            polished_sentence = []
            for stext in sentence:
                for token in stext.tokens:
                    stext.tokens.remove(token) if token.isspace() else print(end='')
                if len(stext.tokens) != 0:
                    polished_sentence.append(stext)
            polished_new_sentences.append(polished_sentence)

        for sentence in polished_new_sentences:
            start_index = 0
            for stext in sentence:
                end_index = start_index + len(stext.tokens) - 1
                stext.token_start_index = start_index
                stext.token_end_index = end_index
                start_index = end_index + 1
            output = sentenceToJson(sentence, spatial_entities, links)
            if output != '':
                json += output + ','
                jsonAll += output + ','
        json = json[:-1] + ']'
        if json != ']':
            saveTextInFile(file_directory, json, '.json')
    jsonAll = jsonAll[:-1] + ']'
    saveTextInFile(SAVE_AS, jsonAll, '.json')
    json_types = getTypesInJsonDynamic()
    saveTextInFile('types', json_types, '.json')
    
    

    


def getSpecificFileDirectories(in_folder_directory, file_type):
    """
    returns all file directories in a directory with a specific fileType
    """
    file_names = []
    for file in  os.listdir(in_folder_directory):
        curr_dir = in_folder_directory + file
        filename, file_extension = os.path.splitext(curr_dir)
        if not os.path.isfile(curr_dir):
            file_names += getSpecificFileDirectories(curr_dir + '\\', file_type)
        elif file_extension == file_type:
            file_names += [curr_dir]
    return file_names


def getTextAndInfosFromXmlFile(directory):
    """
    returns the text from the exisiting xml files
    """
    print(directory)
    tree = ET.parse(directory)
    root = tree.getroot()
    text = root[0].text
    start_by_id = {}
    end_by_id = {}
    start_ends = []

    spatial_entities = []
    json_rels = []
    
    for node in root[1]:
        try:
            node_id = node.get('id')
            start = node.get('spans').split('~')[0]
            end = node.get('spans').split('~')[1]
            start_by_id[node_id] = int(start)
            end_by_id[node_id] = int(end)
            start_ends.append([int(start), int(end)])
        except:
             #print('str->int error')
             pass

    for spatial_entity in root[1].findall('SPATIAL_ENTITY'):
        try:
            se = {}
            start = spatial_entity.get('spans').split('~')[0]
            end = spatial_entity.get('spans').split('~')[1]
            raw_type = spatial_entity.get('type')
            entity_type = raw_type if raw_type != '' else 'OTHER'
            se['start'] = int(start)
            se['end'] = int(end)
            se['type'] = entity_type
            if entity_type == None:
                continue
            spatial_entities.append(se)
            ####dynamic####
            if entity_type not in [x[0] for x in se_types_dynamic] and entity_type != None:
                se_types_dynamic.append([entity_type, entity_type])
            ###############
        except:
            # print('str->int error')
            pass

    for json_rel in root[1].findall('OLINK'):
        try:
            j_rel = {}
            from_start_end = json_rel.get('fromID')
            to_start_end = json_rel.get('toID')
            rel_type = json_rel.get('relType')
            j_rel['from_start'] = start_by_id[from_start_end]
            j_rel['from_end'] = end_by_id[from_start_end]
            j_rel['to_start'] = start_by_id[to_start_end]
            j_rel['to_end'] = end_by_id[to_start_end]
            j_rel['relType'] = rel_type
            if rel_type == None:
                continue
            json_rels.append(j_rel)
            ####dynamic####
            if rel_type not in [x[0] for x in rel_types_dynamic] and rel_type != None:
                rel_types_dynamic.append([rel_type, rel_type, 'false'])
            ###############
        except:
            pass
    for json_rel in root[1].findall('QSLINK'):
        try:
            j_rel = {}
            from_start_end = json_rel.get('fromID')
            to_start_end = json_rel.get('toID')
            rel_type = json_rel.get('relType')
            j_rel['from_start'] = start_by_id[from_start_end]
            j_rel['from_end'] = end_by_id[from_start_end]
            j_rel['to_start'] = start_by_id[to_start_end]
            j_rel['to_end'] = end_by_id[to_start_end]
            j_rel['relType'] = rel_type
            if rel_type == None:
                continue
            json_rels.append(j_rel)
            ####dynamic####
            if rel_type not in [x[0] for x in rel_types_dynamic] and rel_type != None:
                rel_types_dynamic.append([rel_type, rel_type, 'false'])
            ###############
        except:
            pass
    ############################################
    for json_rel in json_rels:
        from_se = {}
        to_se = {}
        from_se['start'] = int(json_rel['from_start'])
        from_se['end'] = int(json_rel['from_end'])
        from_se['type'] = 'OTHER'
        to_se['start'] = int(json_rel['to_start'])
        to_se['end'] = int(json_rel['to_end'])
        to_se['type'] = 'OTHER'
        if [from_se['start'], from_se['end']] not in [[x['start'], x['end']] for x in spatial_entities]:
            spatial_entities.append(from_se)
        if [to_se['start'], to_se['end']] not in [[x['start'], x['end']] for x in spatial_entities]:
            spatial_entities.append(to_se)
    ############################################
    return text, start_ends, spatial_entities, json_rels

def splitText(stext, start_ends):
    """
    splits the subText into further subTextes with the start and end form start_ends
    and returns a sorted list of it
    """
    text_list = [stext]
    for start, end in start_ends:
        for sub_text in text_list:
            sub_text.active = True
        for sub_text in text_list:
            if sub_text.active == False:
                continue
            start_index = start - sub_text.start
            end_index = end - sub_text.start
            if sub_text.start <= start and sub_text.end >= end:
                if start_index > 0:
                    left_text = subText(sub_text.text[0:start_index], sub_text.start, start)
                    text_list.append(left_text)
                if end_index < len(sub_text.text):
                    right_text = subText(sub_text.text[end_index:], end, sub_text.end)
                    text_list.append(right_text)
                mid_text = subText(sub_text.text[start_index:end_index], start, end)
                text_list.append(mid_text)
                text_list.remove(sub_text)
            elif end > sub_text.end and sub_text.end >= start and sub_text.start < start:
                if start_index > 0:
                    left_text = subText(sub_text.text[0:start_index], sub_text.start, start)
                    text_list.append(left_text)
                right_text = subText(sub_text.text[start_index:], start, sub_text.end)
                text_list.append(right_text)
                text_list.remove(sub_text)
            elif end >= sub_text.start and sub_text.end > end and sub_text.start > start:
                if end_index < len(sub_text.text):
                    right_text = subText(sub_text.text[end_index:], end, sub_text.end)
                    text_list.append(right_text)
                left_text = subText(sub_text.text[0:end_index], sub_text.start, end)
                text_list.append(left_text)
                text_list.remove(sub_text)
    text_list.sort(key=lambda x: x.start, reverse=False)
    return text_list

def splitToSentences(text):
    """
    Splits a text into sentences and returns the list of it
    """
    sentence_list = []
    sentence_ends = [m.start() for m in re.finditer('(\.|\?|!)', text)]
    start = 0
    for sentence_end in sentence_ends:
        end = sentence_end+1
        sentence = text[start:end]
        sentence_list.append(subText(sentence, start, end))
        start = end
    return sentence_list


def textToTokens(raw_text):
    """
    returns a list of tokens for the given text (raw_text)
    """
    doc = NLP(raw_text.replace('\n',' ').replace('\"',''))
    return [token.text for token in doc]

def sentenceToJson(sentence, spatial_entities, json_rels):
    """
    returns the requested json for the given sentence in the spert format
    """
    json = '{'
    tokenlist = []
    for stext in sentence:
        for token in stext.tokens:
            tokenlist.append([token])
    json += jsonCreatorHelper('tokens', [], tokenlist, []) + ', '
    se_values = []
    SE_KEYS = ['start', 'end', 'type']
    se_datatypes = ['int', 'int', 'string']
    for spatial_entity in spatial_entities:
        start = None
        end = None
        for stext in sentence:
            if stext.start == spatial_entity['start']:
                start = stext.token_start_index
            if stext.end == spatial_entity['end']:
                end = stext.token_end_index
        if start != None and end != None:
            #+1 to fix the bug
            se_values.append([start, end+1, spatial_entity['type']])
    json += jsonCreatorHelper('entities', SE_KEYS, se_values, se_datatypes) + ', '
    qsv_values = []
    QSV_KEYS = ['head', 'tail', 'type']
    qsv_datatypes = ['int', 'int', 'string']
    for json_rel in json_rels:
        from_start = None
        from_end = None
        to_start = None
        to_end = None
        for stext in sentence:
            if stext.start == json_rel['from_start']:
                from_start = stext.token_start_index
            if stext.end == json_rel['from_end']:
                from_end = stext.token_end_index
            if stext.start == json_rel['to_start']:
                to_start = stext.token_start_index
            if stext.end == json_rel['to_end']:
                to_end = stext.token_end_index
        if from_start != None and from_end != None and to_start != None and to_end != None:
            from_start = \
            [nr for nr in range(len(se_values)) if se_values[nr][0] == from_start and se_values[nr][1] == from_end + 1][0]
            to_start = \
            [nr for nr in range(len(se_values)) if se_values[nr][0] == to_start and se_values[nr][1] == to_end + 1][0]
            qsv_values.append([from_start, to_start, json_rel['relType']])

    json += jsonCreatorHelper('relations', QSV_KEYS, qsv_values, qsv_datatypes)
    global cur_index
    json += ', \"doc\": ' + '\"Sentence'+str(cur_index)+'\"'
    cur_index += 1
    json += '}'
    if se_values == [] and qsv_values == []:
        return ''
    else:
        return json
        

def jsonCreatorHelper(name, keys, values, datatypes):
    """
    #keys = #values[i] = #datatypes -> 0 <= i < len(values)
    """
    if len(values) > 0:
        json = '\"' + name + '\": ['
        if len(keys) == 0:
            for value in values:
                json += '\"' + value[0] + '\"' + ','
        else:
            for value in values:
                sub_json = '{'
                for i in range(0, len(keys)):
                    if datatypes[i] == 'string':
                        sub_json += '\"' + keys[i] + '\": ' + '\"' + str(value[i]) + '\",'
                    elif datatypes[i] == 'int':
                        sub_json += '\"' + keys[i] + '\": ' + str(value[i]) + ','
                json += sub_json[0:-1] + '}' + ','
        json = json[0:-1] + ']'
        return json
    else:
        return '\"' + name + '\": []'

def jsonCreatorHelper2(name, keys, values, datatypes):
    """
    #keys = #values[i] = #datatypes -> 0 <= i < len(values)
    """
    if len(values) > 0:
        json = '\"' + name + '\": '
        if len(keys) == 0:
            for value in values:
                json += '\"' + value[0] + '\"' + ','
        else:
            for value in values:
                sub_json = '{'
                for i in range(0, len(keys)):
                    if datatypes[i] == 'string':
                        sub_json += '\"' + keys[i] + '\": ' + '\"' + str(value[i]) + '\",'
                    else: #datatypes[i] == 'int': | boolean
                        sub_json += '\"' + keys[i] + '\": ' + str(value[i]) + ','
                json += sub_json[0:-1] + '}' + ','
        json = json[0:-1] + ''
        return json
    else:
        return '\"' + name + '\": '

def getTypesInJsonDynamic():
    """
    returns a types files for the entity and relation files, which were found in the xml file
    """
    se_type_names = ['short', 'verbose']
    se_datatypes = ['string', 'string']
    rel_type_names = ['short', 'verbose', 'symmetric']
    rel_datatypes = ['string', 'string', 'boolean']
    json = '{\"entities\": {'
    for se_type in se_types_dynamic:
        json += jsonCreatorHelper2(se_type[0], se_type_names, [se_type], se_datatypes) + ','
    json = json[:-1] + '},'
    json += '\"relations\": {'
    for qsl_type in rel_types_dynamic:
        json += jsonCreatorHelper2(qsl_type[0], rel_type_names, [qsl_type], rel_datatypes) + ','
    json = json[:-1] + '}'
    json += '}'
    return json


def saveTextInFile(directory, text, file_type):
    """
    saves the given json sentences in a json file
    """
    file = directory.split('\\')[-1]
    filename, file_extension = os.path.splitext(file)
    save_in = RESULT_FOLDER + filename + file_type
    f = open(save_in, 'w', encoding='utf-8')
    f.write(text)
    f.close()

if __name__ == '__main__':
    main()

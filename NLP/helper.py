def remove_duplicates_in_list(lst):
    """
    Removes the given list without of any dublicates
    """
    new_lst = []
    for element in lst:
        if element not in new_lst:
            new_lst.append(element)
    return new_lst


def starts_with_item_of_list(result, lst):
    """
    Checks if the result string starts with any element of the list
    """
    for item in lst:
        if result.startswith(item):
            return True
    return False


def get_unique_relations(begin_str, end_str, type_str, rel_lst):
    """
    Removes all sub relations of an already existing relation in the list
    """
    rels_duplicates = []
    for rel in rel_lst:
        parts = find_sub_part(begin_str, end_str, type_str, rel_lst, rel[begin_str], rel[begin_str], rel[type_str], rel[begin_str], rel[end_str])
        if parts is not None:
            rels_duplicates += parts

    for rel_dup in rels_duplicates:
        for rel_nr in range(len(rel_lst)):
            rel = rel_lst[rel_nr]
            if rel_dup[begin_str] == rel[begin_str] and rel_dup[end_str] == rel[end_str]:
                del rel_lst[rel_nr]
                break

    return rel_lst


def find_sub_part(begin_str, end_str, type_str, rel_lst, curr_begin, curr_end, type_search, begin_search, end_search):
    """
    Recursive steps of the function "remove_parts_of_relation"
    """
    for rel in rel_lst:
        if rel[begin_str] == begin_search and rel[end_str] == end_search and rel[type_str] == type_search:
            continue
        if curr_end == rel[begin_str] and end_search == rel[end_str] and rel[type_str] in type_search:
            return [rel]
        if curr_end == rel[begin_str] and rel[type_str] in type_search:
            parts = find_sub_part(begin_str, end_str, type_str, rel_lst, curr_end, rel[end_str], type_search, begin_search, end_search)
            if parts is not None:
                return [rel] + parts
            else:
                return None
    return None
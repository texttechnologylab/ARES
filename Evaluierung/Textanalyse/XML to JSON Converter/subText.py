"""
Helper class which saves a subpart of a text with the according information
"""

class subText():
    
    def __init__(self, text, start, end):
        self.text = text
        self.start = start
        self.end = end
        self.active = False
        self.tokens = []
        self.token_start_index = None
        self.token_end_index = None

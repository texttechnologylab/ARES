import traceback
from flask import Flask, request, jsonify

from output import SpacyOutputHandler
from logger import Logger

log = Logger()
app = Flask(__name__)

def extract_info(text):
    soh = SpacyOutputHandler(text)
    soh.spacy_dependency_tagger()
    soh.spacy_pos_tagger()
    soh.spacy_token_tagger()
    soh.fix_lemma()
    return soh.create_output()


@app.route('/', methods=['POST'])
def get_input_text():
    try:
        text = request.form['inputText']
        model = request.form['model']
        if model == "SpaCy":
            output = extract_info(text)
            input_message = log.transaction_text(text, output)
            log.log_new_transaction(input_message)
            return jsonify(output)
        else:
            return 'Model invalid!'
    except Exception:
        error_message = log.transaction_text(traceback.format_exc())
        log.log_new_transaction(error_message)
        return 'Request invalid!'

from datetime import datetime

LOG_PATH = 'Logs/'

class Logger:

    def __init__(self):
        self.log_name = self.get_current_datetime()
        self.create_main_log(self.log_name)


    def get_current_datetime(self):
        dt = datetime.now()
        dt_string = "%s-%s-%s_%s-%s-%s" % (dt.day, dt.month, dt.year, dt.hour, dt.minute, dt.second)
        return dt_string

    def create_main_log(self, name):
        with open(LOG_PATH + name + '.txt', 'w') as f:
            f.write('Creation: ' + name + '\n')
            f.write('Following transactions occurred during runtime:\n')

    def transaction_text(self, result):
        text = '#' * 25 + self.get_current_datetime() + '#' * 25 + '\n'
        text += str(result) + '\n'
        return text

    def transaction_text(self, input_text, output_text):
        text = '#' * 25 + self.get_current_datetime() + '#' * 25 + '\n'
        text += 'input:' + '\n'
        text += str(input_text) + '\n'
        text += 'output:' + '\n'
        text += str(output_text) + '\n'
        return text

    def log_new_transaction(self, transaction):
        with open(LOG_PATH + self.log_name + '.txt', 'a') as f:
            f.write(transaction)
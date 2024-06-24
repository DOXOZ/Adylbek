import pandas as pd
import telebot
from telebot import types
import gspread
from google.auth.transport.requests import Request
from google.oauth2.service_account import Credentials
import schedule
import threading
import time

# Список необходимых прав доступа (scopes)
scope = ["https://www.googleapis.com/auth/spreadsheets", "https://www.googleapis.com/auth/drive"]

# Загрузка учетных данных из файла JSON
creds = Credentials.from_service_account_file(r'C:\Users\User\Desktop\Minor_bot\minor_test.json', scopes=scope)
    
# Обновление учетных данных, если они истекли
if creds and creds.expired and creds.refresh_token:
    creds.refresh(Request())
else:
    creds = creds

# Авторизация в Google Sheets
client = gspread.authorize(creds)

spreadsheet = client.open("NewDatabase1")
sheet1 = spreadsheet.worksheet()[0]
sheet2 = spreadsheet.worksheet()[1]
sheet3 = spreadsheet.worksheet()[2]

token = "6427760411:AAHU7_llxbRG6-0kgiiR-pVWahFZpoUegiI"
bot = telebot.TeleBot(token)
df = pd.DataFrame.from_dict(sheet1.get_all_records())
df1 = pd.DataFrame.from_dict(sheet2.get_all_records())
df2 = pd.DataFrame.from_dict(sheet3.get_all_records())
users = set()

firstcourse = set(["1 курс", "Посвящение 1", "Ишеним Булагы 1", "Стажировка", "Клубы", "Часы вне колледжа", "Оринтейшн 1"])
secondcourse = set(["2 курс", "Посвящение 2", "Ишеним Булагы 2", "Стажировка","Клубы", "Часы вне колледжа", "Оринтейшн 2"])
thirdcourse = set(["3 курс", "Посвящение 3", "Ишеним Булагы 3", "Стажировка","Клубы", "Часы вне колледжа", "Оринтейшн 3"])

def update_df():
    global df
    '''
    Функция создана для обновления датафрейма
    '''
    df = pd.DataFrame.from_dict(sheet1.get_all_records())
    df1 = pd.DataFrame.from_dict(sheet2.get_all_records())
    df2 = pd.DataFrame.from_dict(sheet3.get_all_records())

schedule.every(1).minutes.do(update_df)

def run_schedule():
    while True:
        schedule.run_pending()
        time.sleep(1)

# Запуск планировщика в отдельном потоке
thread = threading.Thread(target=run_schedule)
thread.start()

# Словарь для хранения мероприятий
events_dict = {}

# Словарь для хранения списков участников мероприятий
adding_minorh = {}

# Флаг для отслеживания добавления мероприятия
adding_event = {}

# Флаг для отслеживания удаления мероприятия
removing_event = {}

# Функция для создания инлайн-клавиатуры для каждого мероприятия

waiting_for_id = {}

#флаг для добавления майнор часов 
add_minor = {}

commands_enabled = False

def append_hours(df, course, message, sheetname): 
    inputed = list(map(str, message.text.split(";")))
    if len(inputed)!=3:
        add_minor[message.chat.id] = False
        adding_minorh[message.chat.id] = False
    if inputed[2] in ["Ишеним Булагы 2023-2024","Посвящение 2023","Лекции", "Стажировка", "Часы вне колледжа", "Клубы"]:
        if df['ФИО'].str.contains(inputed[0]).sum() == 1:
            df.loc[df['ФИО'].str.contains(inputed[0]), inputed[2]] += int(inputed[1])
            df.loc[df['ФИО'].str.contains(inputed[0]), 'Events'] += f' {inputed[2]}({inputed[1]}),'
            sheetname.update(values=[df.columns.values.tolist()] + df.values.tolist(), range_name='A1')
            bot.send_message(message.chat.id, "добавил")
        else:
            keyss = list(df.loc[df['ФИО'].str.contains(inputed[0]), 'ФИО'])
            markup = types.ReplyKeyboardMarkup(resize_keyboard=True, one_time_keyboard=True)
            btn1 = types.KeyboardButton('Back to menu')
            markup.add(btn1)
            for i in keyss:
                btn = types.KeyboardButton(f"{i};{inputed[1]};{inputed[2]}")
                markup.add(btn)
            adding_minorh[message.chat.id] = True
            bot.send_message(message.chat.id, "Выберите:", reply_markup=markup)
    elif df['ФИО'].str.contains(inputed[0]).sum() != 0:
        if df['ФИО'].str.contains(inputed[0]).sum() == 1:
            df.loc[df['ФИО'].str.contains(inputed[0]), '2023-2024'] += int(inputed[1])
            df.loc[df['ФИО'].str.contains(inputed[0]), 'Events'] += f' {inputed[2]}({inputed[1]}),'
            sheet1.update(values=[df.columns.values.tolist()] + df.values.tolist(), range_name='A1')
            bot.send_message(message.chat.id, "добавил")
        else:
            keyss = list(df.loc[df['ФИО'].str.contains(inputed[0]), 'ФИО'])
            markup = types.ReplyKeyboardMarkup(resize_keyboard=True, one_time_keyboard=True)
            btn1 = types.KeyboardButton('Back to menu')
            markup.add(btn1)
            for i in keyss:
                btn = types.KeyboardButton(f"{i};{inputed[1]};{inputed[2]}")
                markup.add(btn)
            adding_minorh[message.chat.id] = True
            bot.send_message(message.chat.id, "Выберите:", reply_markup=markup)

@bot.message_handler(commands=['start'])
def start_message(message):
    global commands_enabled
    markup = types.ReplyKeyboardMarkup(resize_keyboard=True)
    btn1 = types.KeyboardButton(text="Узнать сколько часов", request_contact=True)
    btn2 = types.KeyboardButton("Посмотреть список мероприятий")
    markup.add(btn1, btn2)
    bot.send_message(message.chat.id, "Привет ✌️", reply_markup=markup)
    commands_enabled = False
    
@bot.message_handler(content_types=['contact'])
def normal(message):
    global df
    if message.contact is not None:
        try:
            usr_id = int(message.contact.phone_number)
            user = message.from_user
            if usr_id in df["Номер сотового телефона"].values and message.contact.user_id == user.id:
                bot.send_message(message.chat.id, f'{df[df["Номер сотового телефона"] == usr_id]["ИТОГО ЧАСОВ"].values[0]}')
            else:
                bot.send_message(message.chat.id, 'Нету такого номера телефона')
        except:
            bot.send_message(message.chat.id, 'Ошибка')

@bot.message_handler(func=lambda message: message.text == "/228admin")
def azemkrasotka(message):
    global commands_enabled
    global df
    markup = types.ReplyKeyboardMarkup(resize_keyboard=True)
    btn1 = types.KeyboardButton("Добавить мероприятие")
    btn2 = types.KeyboardButton("Удалить мероприятие")
    btn3 = types.KeyboardButton("Добавить часы по ФИО")
    markup.add(btn1, btn2, btn3)
    commands_enabled = True
    bot.send_message(message.chat.id, "Выберите действие:", reply_markup=markup)

@bot.message_handler(func=lambda message: message.text == "Добавить часы по ФИО")
def add_minorh(message):
    global commands_enabled
    if commands_enabled:
        bot.send_message(message.chat.id, "ФИО;часы;мероприятие")
        add_minor[message.chat.id] = True

@bot.message_handler(func=lambda message: add_minor.get(message.chat.id, False))
def add_minor_hours(message):
    global commands_enabled
    global df
    if commands_enabled:
        if message.text.lower() == "stop":
            add_minor[message.chat.id] = False
            adding_minorh[message.chat.id] = False
        else:
                inputed = list(map(str, message.text.split(";")))
                if len(inputed)!=3:
                    add_minor[message.chat.id] = False
                    adding_minorh[message.chat.id] = False
                if inputed[2] in ["Ишеним Булагы 2023-2024","Посвящение 2023","Лекции", "Стажировка", "Часы вне колледжа", "Клубы"]:
                    if df['ФИО'].str.contains(inputed[0]).sum() == 1:
                        df.loc[df['ФИО'].str.contains(inputed[0]), inputed[2]] += int(inputed[1])
                        df.loc[df['ФИО'].str.contains(inputed[0]), 'Events'] += f' {inputed[2]}({inputed[1]}),'
                        sheet1.update(values=[df.columns.values.tolist()] + df.values.tolist(), range_name='A1')
                        bot.send_message(message.chat.id, "добавил")
                    else:
                        keyss = list(df.loc[df['ФИО'].str.contains(inputed[0]), 'ФИО'])
                        markup = types.ReplyKeyboardMarkup(resize_keyboard=True, one_time_keyboard=True)
                        btn1 = types.KeyboardButton('Back to menu')
                        markup.add(btn1)
                        for i in keyss:
                            btn = types.KeyboardButton(f"{i};{inputed[1]};{inputed[2]}")
                            markup.add(btn)
                        adding_minorh[message.chat.id] = True
                        bot.send_message(message.chat.id, "Выберите:", reply_markup=markup)
                elif df['ФИО'].str.contains(inputed[0]).sum() != 0:
                    if df['ФИО'].str.contains(inputed[0]).sum() == 1:
                        df.loc[df['ФИО'].str.contains(inputed[0]), '2023-2024'] += int(inputed[1])
                        df.loc[df['ФИО'].str.contains(inputed[0]), 'Events'] += f' {inputed[2]}({inputed[1]}),'
                        sheet1.update(values=[df.columns.values.tolist()] + df.values.tolist(), range_name='A1')
                        bot.send_message(message.chat.id, "добавил")
                    else:
                        keyss = list(df.loc[df['ФИО'].str.contains(inputed[0]), 'ФИО'])
                        markup = types.ReplyKeyboardMarkup(resize_keyboard=True, one_time_keyboard=True)
                        btn1 = types.KeyboardButton('Back to menu')
                        markup.add(btn1)
                        for i in keyss:
                            btn = types.KeyboardButton(f"{i};{inputed[1]};{inputed[2]}")
                            markup.add(btn)
                        adding_minorh[message.chat.id] = True
                        bot.send_message(message.chat.id, "Выберите:", reply_markup=markup)
                else:
                    bot.send_message(message.chat.id, "Такого человека нету в списке")
            
@bot.message_handler(func=lambda message: adding_minorh.get(message.chat.id, False))
def confirm_adding_minorh(message):
    global commands_enabled
    global df
    try:
        if commands_enabled:
                    if message.text.lower() == "stop":
                        add_minor[message.chat.id] = False
                        adding_minorh[message.chat.id] = False
                    else:
                        add_minor[message.chat.id] = False
                        add_minor_hours(message)
                        adding_minorh[message.chat.id] = False
    except:
                bot.send_message(message.chat.id, "Что-то не так")
        

@bot.message_handler(func=lambda message: message.text == "Добавить мероприятие")
def add_event(message):
    global commands_enabled
    if commands_enabled: 
        bot.send_message(message.chat.id, "Имя;описание")
        adding_event[message.chat.id] = True

@bot.message_handler(func=lambda message: adding_event.get(message.chat.id, False))
def add_event_text(message):
    global commands_enabled
    if commands_enabled:
        event_text = list(map(str, message.text.split(';')))
        try:
            next_event_key = event_text[0]
            events_dict[next_event_key] = event_text[1]
        except:
            bot.send_message(message.chat.id, "У вас нету ';' в сообщении")

        for i in users:
            bot.send_message(i, f"Добавлено новое мероприятие!!!")
        adding_event[message.chat.id] = False

@bot.message_handler(func=lambda message: message.text == "Удалить мероприятие")
def remove_event(message):
    global commands_enabled
    if commands_enabled:
        if not events_dict:
            bot.send_message(message.chat.id, "Список мероприятий пуст.")
        else:
            keys = [str(key) for key in events_dict.keys()]
            markup = types.ReplyKeyboardMarkup(resize_keyboard=True)
            btn1 = types.KeyboardButton('Back to menu')
            markup.add(btn1)
            for key in keys:
                btn = types.KeyboardButton(key)
                markup.add(btn)
            bot.send_message(message.chat.id, "Выберите мероприятие для удаления:", reply_markup=markup)
            removing_event[message.chat.id] = True

@bot.message_handler(func=lambda message: message.text == "Back to menu")
def BackToMenu(message):
    if commands_enabled:
        azemkrasotka(message)
        removing_event[message.chat.id] = False

@bot.message_handler(func=lambda message: removing_event.get(message.chat.id, False))
def confirm_removal(message):
    global commands_enabled
    if commands_enabled:
        key_to_remove = message.text
        if key_to_remove in events_dict:
            removed_event = events_dict.pop(key_to_remove)
            bot.send_message(message.chat.id, f"Мероприятие {key_to_remove} удалено:\n{removed_event}")
        else:
            bot.send_message(message.chat.id, "Такого мероприятия не существует.")

@bot.message_handler(func=lambda message: message.text == "Посмотреть список мероприятий")
def list_events(message):
    if not events_dict:
        bot.send_message(message.chat.id, "Список мероприятий пуст.")
    else:
        events_list = "\n".join([f"{key}: {event_text}" for key, event_text in events_dict.items()])
        bot.send_message(message.chat.id, f"Список мероприятий:\n{events_list}")

bot.infinity_polling(none_stop=True)
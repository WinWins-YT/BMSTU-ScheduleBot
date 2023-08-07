# ScheduleBot

Этот бот работает в ВК и выдает расписание либо по запросу пользователя на определенный день, либо по времени, которое выставляет сам пользователь

Весь проект написан на .NET 6.0 и для его запуска необходимо установить этот пакет (dotnet-runtime-6.0 для Linux или [для Windows и Linux](https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime))

### ScheduleBot.ExcelParser

###### Для запуска:

	git clone https://github.com/WinWins-YT/BMSTU-ScheduleBot
	cd BMSTU-ScheduleBot/ScheduleBot.ExcelParser
    dotnet run

###### Файл настроек

После компиляции будет создан шаблонный файл `settings.json` в папке `bin/Debug/net6.0`, рекомендуется закрыть приложение с помощью `Ctrl-C` и проверить этот файл

Содержимое файла `settings.json`:

```json
{
    "NumberOfLessonsPerDay": [
        [5, 4, 4, 4, 4, 3],
        [5, 4, 4, 5, 5, 3],
        [5, 5, 5, 5, 6, 3],
        [7, 6, 6, 7, 5, 3],
        [5, 4, 5, 5, 4, 3],
        [3, 5, 5, 3, 3, 2],
        [4, 4, 4, 5, 4, 3],
        [4, 4, 3, 3, 6, 3]
    ],
    "ColumnCount": [29, 28, 25, 25, 9, 6, 15, 13]
}
```

`NumberOfLessonsPerDay` тип `int[][]` - количество прописанных пар на каждый день на каждой странице. То есть итого должно быть 8 массивов на каждую страницу файла по 6 чисел на каждый день недели

`ColumnCount` тип `int[]` - количество столбцов на каждой странице, итого 8 чисел

###### Как работает

Этот проект переводит таблицу Excel с расписанием в JSON файл для дальнейшего использования

При запуске предлагается указать путь к .xlsx файлу с расписанием. После этого будет выводится список всех полученных групп из этого файла. Затем в папке `bin/Debug/net6.0` создатся файл `schedule.json`

Поддерживаются только файлы формата OpenOffice XML (Office 2007-2021) *.xlsx

### ScheduleBot.VkBot

###### Для запуска:

	git clone https://github.com/WinWins-YT/BMSTU-ScheduleBot
	cd BMSTU-ScheduleBot/ScheduleBot
	dotnet build

Скопировать файл `schedule.json`, который сформировал ScheduleBot.ExcelParser, файл `settings.json` с настройками, и, если есть, файл `users.json`, который содержит зарегистрированных пользователей и их группы, в bin/Debug/net6.0

	dotnet bin/Debug/net6.0/ScheduleBot.dll

###### Файл настроек

Содержимое файла `settings.json`

```json
{
    "Token": "vk1...",
    "GroupUrl": "https://vk.com/...",
    "SemesterStart": "01.01.1970"
}
```

`Token` тип `string` - токен для ВК бота, полученный в настройках сообщества

`GroupUrl` тип `string` - URL группы (https://vk.com/...)

`SemesterStart` тип `DateTime` -  число понедельника недели на которой будет начало семестра в формате (ЧЧ.ММ.ГГГГ), например, начало семетра 01.09.2022 (четверг), ставим 29.08.2022 (понедельник)
	


###### Как работает

При запуске бот загружает конфигурационные файлы. После появления надписи `Startup successful` бот запустился и работает.

### ScheduleAPI

###### Для запуска

	git clone https://github.com/WinWins-YT/BMSTU-ScheduleBot
	cd BMSTU-ScheduleBot/ScheduleAPI
	dotnet build

Скопировать файл `schedule.json`, который сформировал ScheduleToJSON в bin/Debug/net6.0

	dotnet bin/Debug/net6.0/ScheduleAPI.dll
	
###### Как работает

Это API для получения расписания посредством HTTP GET запросов. Список методов и тестовый стенд можно увидеть по адресу http://localhost/swagger

GET /api/v1 => Список организаций (Студенты)

GET /api/v1/{org} => Список курсов ({org} = Студенты)

GET /api/v1/{org}/{course} => Список групп данного курса ({org} = Студенты, {course} = 1 курс)

GET /api/v1/{org}/{course}/{group}/schedule => Расписание данной группы ({org} = Студенты, {course} = 1 курс, {group} = ИУК2-11Б)



# Приятного аппетита

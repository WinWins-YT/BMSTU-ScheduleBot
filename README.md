# ScheduleBot

Этот бот работает в ВК и выдает расписание либо по запросу пользователя на определенный день, либо по времени, которое выставляет сам пользователь

Весь проект написан на .NET 6.0 и для его запуска необходимо установить этот пакет (dotnet-runtime-6.0 для Linux или [для Windows и Linux](https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime))

### ScheduleToJSON

###### Для запуска:

	git clone https://github.com/WinWins-YT/BMSTU-ScheduleBot
	cd BMSTU-ScheduleBot\ScheduleToJSON

Открыть файл ScheduleToJSON.csproj в Visual Studio и запустить или скомпилировать в .NET Framework

###### Как работает

Этот проект переводит таблицу Excel с расписанием в JSON файл для дальнейшего использования

Здесь используется COM библиотека Excel, поэтому запускать это следует только на Windows с установленным пакетом Microsoft Office

При запуске предлагается указать путь к .xls файлу с расписанием. После этого будет выводится список всех полученных групп из этого файла. Затем создастся в папке `\bin\Debug\net6.0-windows` файл `schedule.json`

### ScheduleBot

###### Для запуска:

	git clone https://github.com/WinWins-YT/BMSTU-ScheduleBot
	cd BMSTU-ScheduleBot/ScheduleBot
	dotnet build

Скопировать файл `schedule.json`, который сформировал ScheduleToJSON, файл `settings.json` с настройками, и, если есть, файл `users.json`, который содержит зарегистрированных пользователей и их группы

	dotnet bin/Debug/net6.0/ScheduleBot.dll

###### Файл настроек

Содержимое файла `settings.json`

	{
		"Token": "Токен от ВК",
		"GroupUrl": "URL группы (https://vk.com/...",
		"AdminUsers": [
			Список ID пользователей, которым будут приходить в личку ошибки и в будущем от них будут приниматься административные команды
		]
	}
	


###### Как работает

При запуске бот загружает файлы расписания и, если есть, пользователей. После появления надписи `Initialized` бот запустился и работает.

### ScheduleAPI

###### Для запуска

	git clone https://github.com/WinWins-YT/BMSTU-ScheduleBot
	cd BMSTU-ScheduleBot/ScheduleAPI
	dotnet build

Скопировать файл `schedule.json`, который сформировал ScheduleToJSON

	dotnet bin/Debug/net6.0/ScheduleAPI.dll
	
###### Как работает

Это API для получения расписания посредством HTTP GET запросов. Список методов и тестовый стенд можно увидеть по адресу http://localhost/swagger

GET /api/v1 => Список организаций (Студенты)

GET /api/v1/{org} => Список курсов ({org} = Студенты)

GET /api/v1/{org}/{course} => Список групп данного курса ({org} = Студенты, {course} = 1 курс)

GET /api/v1/{org}/{course}/{group}/schedule => Расписание данной группы ({org} = Студенты, {course} = 1 курс, {group} = ИУК2-11Б)



# Приятного аппетита

# Contributing

When working on the workshops, we advise that you review your changes locally before committing them.

Use ```mkdocs serve``` live preview your changes (as you type) on your local machine.

Use ```deploy.sh``` to deploy when you have completed your changes.

## To setup the first time
```
cd workshops
python3 -m venv venv
source venv/bin/activate
cd buildtools
source mkddocs-setup.sh
```

## Each new session setup
```
cd workshops
source venv/bin/activate
```

## To debug
```
mkdocs serve
```

## To deploy
```
source mkdocs-deploy.sh
```
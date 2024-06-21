sudo apt install -y pipenv
pipenv --clear
sudo apt install -y python3.12-venv
rm -rf env
python3 -m venv env
source env/bin/activate
pip3 install requests "Flask <3" "Werkzeug <3" serverless-wsgi
deactivate
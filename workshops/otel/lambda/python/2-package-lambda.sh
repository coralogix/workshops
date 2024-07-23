rm -rf lambda_package.zip function.zip
mkdir lambda_package
cp lambda_function.py cp lambda_package
cp -r env/lib/python3.12/site-packages/* lambda_package/  # Replace '3.x' with your Python version
cd lambda_package
zip -r ../function.zip .
cd ..
rm -rf lambda_package
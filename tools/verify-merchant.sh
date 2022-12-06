#!/bin/bash

CONTAINER_ID=$(docker ps -q --filter name=payments-mysql)

docker exec -i $CONTAINER_ID mysql -u oscar -pgitCheckout2022! payments_api --init-command="SET @username='$1'" < ./tools/verify-merchant.sql
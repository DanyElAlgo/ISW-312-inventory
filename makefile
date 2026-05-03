SHELL := /bin/bash
.PHONY: all inventory sales

all:
	$(MAKE) -C Inventory.API &
	$(MAKE) -C Sales.API &

inventory:
	$(MAKE) -C Inventory.API

sales:
	$(MAKE) -C Sales.API

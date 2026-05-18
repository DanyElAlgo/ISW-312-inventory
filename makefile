SHELL := /bin/bash
.PHONY: all inventory sales purchases

all:
	$(MAKE) -C Inventory.API &
	$(MAKE) -C Sales.API &
	$(MAKE) -C Purchases.API &

inventory:
	$(MAKE) -C Inventory.API

sales:
	$(MAKE) -C Sales.API

purchases:
	$(MAKE) -C Purchases.API

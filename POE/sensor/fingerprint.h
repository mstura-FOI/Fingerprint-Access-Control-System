#pragma once
#include <stdint.h>
#include <stdbool.h>

bool     fp_init(void);
uint16_t fp_enroll(uint8_t *tpl_out, uint16_t cap);        
bool     fp_verify(const uint8_t *tpl_in, uint16_t len, bool *matched);
bool     fp_verify_test(const uint8_t *tpl_in, uint16_t len, bool *matched);
bool     fp_test_store_search(const uint8_t *tpl_in, uint16_t len, bool *matched);
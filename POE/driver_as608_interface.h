#ifndef DRIVER_AS608_INTERFACE_H
#define DRIVER_AS608_INTERFACE_H
#include "driver_as608.h"
#include <stdint.h>
#include <stdarg.h>

#ifdef __cplusplus
extern "C" {
#endif

uint8_t  as608_interface_uart_init(void);
uint8_t  as608_interface_uart_deinit(void);
uint16_t as608_interface_uart_read(uint8_t *buf, uint16_t len);
uint8_t  as608_interface_uart_write(uint8_t *buf, uint16_t len);
uint8_t  as608_interface_uart_flush(void);
void     as608_interface_delay_ms(uint32_t ms);
void     as608_interface_debug_print(const char *const fmt, ...);

#ifdef __cplusplus
}
#endif

#endif
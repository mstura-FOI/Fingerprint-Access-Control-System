#include "driver_as608_interface.h"
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include "driver/uart.h"
#include "driver/gpio.h" 
#include <stdio.h>
#include <stdarg.h>

#define AS608_UART_NUM     UART_NUM_2
#define AS608_PIN_TX       GPIO_NUM_17
#define AS608_PIN_RX       GPIO_NUM_16
#define AS608_BAUD         57600
#define AS608_RX_BUF       (4096)
#define AS608_TIMEOUT_MS   pdMS_TO_TICKS(6000)

uint8_t as608_interface_uart_init(void) {
    const uart_config_t cfg = {
        .baud_rate  = AS608_BAUD,
        .data_bits  = UART_DATA_8_BITS,
        .parity     = UART_PARITY_DISABLE,
        .stop_bits  = UART_STOP_BITS_1,
        .flow_ctrl  = UART_HW_FLOWCTRL_DISABLE,
        .source_clk = UART_SCLK_DEFAULT,
    };
    if (uart_param_config(AS608_UART_NUM, &cfg) != ESP_OK)       return 1;
    if (uart_set_pin(AS608_UART_NUM, AS608_PIN_TX, AS608_PIN_RX,
                     UART_PIN_NO_CHANGE, UART_PIN_NO_CHANGE) != ESP_OK) return 1;
    if (uart_driver_install(AS608_UART_NUM, AS608_RX_BUF, 0,
                            0, NULL, 0) != ESP_OK)               return 1;
    return 0;
}

uint8_t as608_interface_uart_deinit(void) {
    uart_driver_delete(AS608_UART_NUM);
    return 0;
}

uint16_t as608_interface_uart_read(uint8_t *buf, uint16_t len) {
    int got = uart_read_bytes(AS608_UART_NUM, buf, len, AS608_TIMEOUT_MS);
    return (uint16_t)(got < 0 ? 0 : got);
}

uint8_t as608_interface_uart_write(uint8_t *buf, uint16_t len) {
    int sent = uart_write_bytes(AS608_UART_NUM, (const char *)buf, len);
    return (sent == (int)len) ? 0 : 1;
}

uint8_t as608_interface_uart_flush(void) {
    return (uart_flush(AS608_UART_NUM) == ESP_OK) ? 0 : 1;
}

void as608_interface_delay_ms(uint32_t ms) {
    vTaskDelay(pdMS_TO_TICKS(ms));
}

void as608_interface_debug_print(const char *const fmt, ...) {
    va_list args;
    va_start(args, fmt);
    vprintf(fmt, args);
    va_end(args);
}
#include "code_input.h"
#include "driver/uart.h"
#include "driver/uart_vfs.h"     // ESP-IDF 5.x; za 4.x: "esp_vfs_dev.h" + esp_vfs_dev_uart_use_driver
#include "esp_log.h"
#include <stdio.h>
#include <ctype.h>

void code_input_init(void)
{
    // Blokirajuće čitanje sa serijske konzole (stdin preko UART0).
    uart_driver_install(UART_NUM_0, 256, 0, 0, NULL, 0);
    uart_vfs_dev_use_driver(UART_NUM_0);
    setvbuf(stdin, NULL, _IONBF, 0);
}

bool code_input_read(char *out)
{
    printf("\n>> Utipkaj 6-znamenkasti kod pa Enter: ");
    fflush(stdout);

    char buf[16] = {0};
    if (fgets(buf, sizeof(buf), stdin) == NULL) return false;

    int n = 0;
    for (int i = 0; buf[i] && n < 6; i++)
        if (isdigit((unsigned char)buf[i])) out[n++] = buf[i];
    out[n] = '\0';

    if (n != 6) {
        printf("!! Kod mora imati tocno 6 znamenki.\n");
        return false;
    }
    return true;
}
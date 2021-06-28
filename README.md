# Job_Processador_FIla_Kafka
A cada 2 minutos esse job consome um serviço que é responsável por ler um item da fila do kafka

Se algum item for encontrado na fila, é realizada uma pesquisa em duas planilhas com dados de cotações. Caso o processamento finalize com sucesso, é gerada uma planilha csv com as cotações correspondentes à moeda e o periodo que foi recuperado da fila.

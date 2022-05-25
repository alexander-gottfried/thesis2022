CSTD=-std=c++2a
CC=g++ $(CSTD)

OBJDIR=obj
SRCDIR=.

EXEC=server
EXEC_CS=server_clientsync

BIND_ARGS=-pthread -lboost_thread

OBJS=$(addprefix $(OBJDIR)/, server.o socket_wrapper.o run_server.o)
OBJS_CS=$(addprefix $(OBJDIR)/, server_cs.o socket_wrapper.o run_server.o)

build: servers $(OBJS)
	$(CC) -o $(EXEC) $(OBJS) $(BIND_ARGS)
	$(CC) -o $(EXEC_CS) $(OBJS_CS) $(BIND_ARGS)

upload: build
	sh upload.sh $(EXEC)
	sh upload.sh $(EXEC_CS)

$(OBJDIR)/%.o: $(SRCDIR)/%.cpp
	$(CC) -c $< -o $@

servers: server.cpp
	$(CC) -c server.cpp -o $(OBJDIR)/server.o
	$(CC) -c server.cpp -o $(OBJDIR)/server_cs.o -DCLIENT_SYNC

$(OBJDIR):
	mkdir $(OBJDIR)

clean:
	rm $(OBJDIR)/*.o